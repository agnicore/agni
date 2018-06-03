﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using NFX;
using NFX.Collections;
using NFX.Instrumentation;
using NFX.ServiceModel;

using Agni.Contracts;
using System.Threading.Tasks;
using NFX.Log;

namespace Agni.Workers.Server.Queue
{
  /// <summary>
  /// Glue trampoline for TodoQueueService
  /// </summary>
  public sealed class TodoQueueServer : ITodoQueue
  {
    public int Enqueue(TodoFrame[] todos)
    {
      return TodoQueueService.Instance.Enqueue(todos);
    }
  }

  /// <summary>
  /// Service that enqueues todos
  /// </summary>
  public sealed partial class TodoQueueService : AgentServiceBase, ITodoQueue, ITodoHost
  {

    #region CONSTS
      public const string CONFIG_QUEUE_SECTION = "queue";
      public const string CONFIG_QUEUE_STORE_SECTION = "queue-store";
      public const string CONFIG_TYPE_RESOLVER_SECTION = "type-resolver";

      public const int FULL_BATCH_SIZE = 1024;

      public const string ALL ="*";
    #endregion

    #region STATIC/.ctor

      private static object s_Lock = new object();
      private static volatile TodoQueueService s_Instance;

      internal static TodoQueueService Instance
      {
        get
        {
          var instance = s_Instance;
          if (instance==null) throw new WorkersException("{0} is not allocated".Args(typeof(TodoQueueService).FullName));
          return instance;
        }
      }

      public TodoQueueService() : this(null) {}

      public TodoQueueService(object director) : base(director)
      {
        m_Queues = new Registry<TodoQueue>();

        lock(s_Lock)
        {
          if (s_Instance!=null)
            throw new WorkersException("{0} is already allocated".Args(typeof(TodoQueueService).FullName));

          s_Instance = this;
        }
      }

      protected override void Destructor()
      {
        base.Destructor();
        DisposeAndNull(ref m_QueueStore);
        lock(s_Lock)
          s_Instance = null;
      }

    #endregion

    #region Fields

      private TodoQueueStore      m_QueueStore;
      private Registry<TodoQueue> m_Queues;

      private CappedSet<NFX.DataAccess.Distributed.GDID> m_Duplicates = new CappedSet<NFX.DataAccess.Distributed.GDID>();

      private int               m_stat_EnqueueCalls;
      private NamedInterlocked  m_stat_EnqueueTodoCount       = new NamedInterlocked();
      private int               m_stat_QueueThreadSpins;
      private NamedInterlocked  m_stat_ProcessOneQueueCount   = new NamedInterlocked();

      private NamedInterlocked  m_stat_MergedTodoCount        = new NamedInterlocked();
      private NamedInterlocked  m_stat_FetchedTodoCount       = new NamedInterlocked();
      private NamedInterlocked  m_stat_ProcessedTodoCount     = new NamedInterlocked();


      private NamedInterlocked  m_stat_PutTodoCount            = new NamedInterlocked();
      private NamedInterlocked  m_stat_UpdateTodoCount         = new NamedInterlocked();
      private NamedInterlocked  m_stat_CompletedTodoCount      = new NamedInterlocked();
      private NamedInterlocked  m_stat_CompletedOkTodoCount    = new NamedInterlocked();
      private NamedInterlocked  m_stat_CompletedErrorTodoCount = new NamedInterlocked();

      private NamedInterlocked  m_stat_QueueOperationErrorCount    = new NamedInterlocked();

      private NamedInterlocked  m_stat_TodoDuplicationCount  = new NamedInterlocked();

    #endregion

    #region Properties
      public override int ThreadGranularityMs { get { return 25; } }
    #endregion

    #region Public

      void ITodoHost.LocalEnqueue(Todo todo)
      {
        this.Enqueue(new[] { todo });
      }

      void ITodoHost.LocalEnqueue(IEnumerable<Todo> todos)
      {
        this.Enqueue(todos);
      }

      Task ITodoHost.LocalEnqueueAsync(IEnumerable<Todo> todos)
      {
        CheckServiceActive();
        return Task.Factory.StartNew(() => this.Enqueue(todos) );
      }

      public int Enqueue(IEnumerable<Todo> todos)
      {
        if (todos==null || !todos.Any()) return FULL_BATCH_SIZE;

        todos.ForEach( todo => check(todo));

        return this.Enqueue( todos.Select( t => new TodoFrame(t) ).ToArray() );
      }

      public int Enqueue(TodoFrame[] todos)
      {
        CheckServiceActive();
        if (todos == null || todos.Length == 0)
          return FULL_BATCH_SIZE;

        TodoQueueAttribute attr = null;
        foreach (var todo in todos)
        {
          if (!todo.Assigned)
            continue;

          var todoAttr = GuidTypeAttribute.GetGuidTypeAttribute<Todo, TodoQueueAttribute>(todo.Type, AgniSystem.ProcessManager.TodoTypeResolver);

          if (attr==null) attr = todoAttr;

          if (attr.QueueName != todoAttr.QueueName)
            throw new WorkersException(StringConsts.TODO_QUEUE_ENQUEUE_DIFFERENT_ERROR);
        }

        var queue = m_Queues[attr.QueueName];
        if (queue == null)
          throw new WorkersException(StringConsts.TODO_QUEUE_NOT_FOUND_ERROR.Args(attr.QueueName));


        if (InstrumentationEnabled)
        {
          Interlocked.Increment(ref m_stat_EnqueueCalls);
          m_stat_EnqueueTodoCount.IncrementLong(ALL);
          m_stat_EnqueueTodoCount.IncrementLong(queue.Name);
        }

        enqueue(queue, todos);

        return m_QueueStore.GetInboundCapacity(queue);
      }

    #endregion

    #region .pvt
      private void check(Todo todo)
      {
        if (todo == null) throw new WorkersException(StringConsts.ARGUMENT_ERROR + "Todo.ValidateAndPrepareForEnqueue(todo==null)");

        todo.ValidateAndPrepareForEnqueue(null);
      }

      private class _lck{ public Thread TEnqueu; public DateTime Date; public int IsEnqueue; public int IsProc; }
      private Dictionary<string, _lck> m_CorrelationLocker = new Dictionary<string, _lck>(StringComparer.Ordinal);


/*
 * Locking works as follows:
 * ---------------------------
 * All locking is done per CorrelationKey (different CorrelationKey do not inter-lock at all)
 * Whithin the same key:
 *
 *   Any existing Enqueue lock blocks another Enqueue until existing is released
 *   Any existing Enqueue lock returns FALSE for another Process until all Enqueue released
 *
 *   Any existing Processing lock yields next date to Enqueue (+1 sec)
 *   Any Exisiting Processing lock shifts next Date if another Processing lock is further advanced in time
 *
 *
 *   Enqueue lock is reentrant for the same thread.
 *   Processing lock is reentrant regardless of thread ownership
 *
 */



      /// <summary>
      /// Returns the point in time AFTER which the enqueue operation may fetch correlated todos
      /// </summary>
      private DateTime lockCorrelatedEnqueue(string key, DateTime sd)
      {
        if (key==null) key = string.Empty;

        var ct = Thread.CurrentThread;

        uint spinCount = 0;
        while(true)
        {
          lock(m_CorrelationLocker)
          {
            _lck lck;
            if (!m_CorrelationLocker.TryGetValue(key, out lck))
            {
               lck = new _lck { TEnqueu = ct, Date = sd, IsEnqueue = 1, IsProc = 0 };
               m_CorrelationLocker.Add(key, lck);
               return sd;
            }

            if (lck.IsEnqueue==0)
            {
              lck.IsEnqueue = 1;
              lck.TEnqueu = ct;
              return lck.Date.AddSeconds(1);
            }

            if (lck.TEnqueu==ct)//if already acquired by this thread
            {
              lck.IsEnqueue++;
              if (sd<lck.Date) lck.Date = sd;
              return lck.Date;
            }
          }
          if (spinCount<100)
            Thread.SpinWait(500);
          else
            Thread.Yield();

          unchecked {  spinCount++; }
        }
      }

      /// <summary>
      /// Tries to lock the correlated todo instance with the specified scheduled date, true if locked, false, otherwise.
      /// Takes lock IF not enqueue, false otherwise
      /// </summary>
      private bool tryLockCorrelatedProcessing(string key, DateTime sd)
      {
        if (key==null) key = string.Empty;

        lock(m_CorrelationLocker)
        {
          _lck lck;
          if (!m_CorrelationLocker.TryGetValue(key, out lck))
          {
              lck = new _lck { Date = sd, IsEnqueue = 0, IsProc = 1 };
              m_CorrelationLocker.Add(key, lck);
              return true;
          }

          if (lck.IsEnqueue==0)
          {
            lck.IsProc++;
            if (sd>lck.Date) lck.Date = sd;
            return true;
          }

          return false;//lock is enqueue type
        }
      }

      private bool releaseCorrelatedEnqueue(string key)
      {
        if (key==null) key = string.Empty;

        lock(m_CorrelationLocker)
        {
          _lck lck;
          if (!m_CorrelationLocker.TryGetValue(key, out lck)) return false;

          if (lck.IsEnqueue==0) return false;

          if (Thread.CurrentThread!=lck.TEnqueu) return false;//not locked by this thread

          lck.IsEnqueue--;

          if (lck.IsEnqueue>0) return true;

          lck.TEnqueu = null;//release thread

          if (lck.IsProc==0)
            m_CorrelationLocker.Remove(key);

          return true;
        }
      }

      private bool releaseCorrelatedProcessing(string key)
      {
        if (key==null) key = string.Empty;

        lock(m_CorrelationLocker)
        {
          _lck lck;
          if (!m_CorrelationLocker.TryGetValue(key, out lck)) return false;

          if (lck.IsProc==0) return false;

          lck.IsProc--;

          if (lck.IsProc>0) return true;

          if (lck.IsEnqueue==0)
            m_CorrelationLocker.Remove(key);

          return true;
        }
      }

    #endregion

  }
}
