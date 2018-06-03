﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX;
using NFX.Environment;
using NFX.ServiceModel;
using NFX.Instrumentation;

namespace Agni.Locking
{
  /// <summary>
  /// Base for Facades used for calling locking APIs from client code
  /// </summary>
  public abstract class LockManagerBase : ServiceWithInstrumentationBase<object>, ILockManagerImplementation
  {
    #region CONSTS
      private static readonly TimeSpan INSTR_INTERVAL = TimeSpan.FromMilliseconds(3700);

    #endregion

    #region .ctor

      public LockManagerBase() : base() { }

      protected override void Destructor()
      {
        DisposableObject.DisposeAndNull(ref m_InstrumentationEvent);
        base.Destructor();
      }

    #endregion



    #region Fields

      private ConcurrentDictionary<LockSessionID, LockSession> m_Sessions = new ConcurrentDictionary<LockSessionID,LockSession>();


      private bool m_InstrumentationEnabled;
      private NFX.Time.Event m_InstrumentationEvent;

      private NFX.Collections.NamedInterlocked m_Stats = new NFX.Collections.NamedInterlocked();

    #endregion

    #region Properties

        /// <summary>
        /// Implements IInstrumentable
        /// </summary>
        [Config(Default=false)]
        [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOCKING, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
        public override bool InstrumentationEnabled
        {
          get { return m_InstrumentationEnabled;}
          set
          {
             m_InstrumentationEnabled = value;
             if (m_InstrumentationEvent==null)
             {
               if (!value) return;
               m_Stats.Clear();
               m_InstrumentationEvent = new NFX.Time.Event(App.EventTimer, null, e => AcceptManagerVisit(this, e.LocalizedTime), INSTR_INTERVAL);
             }
             else
             {
               if (value) return;
               DisposableObject.DisposeAndNull(ref m_InstrumentationEvent);
               m_Stats.Clear();
             }
          }
        }

      public LockSession this[LockSessionID sid]
      {
        get
        {
           LockSession session;
           if (m_Sessions.TryGetValue(sid, out session)) return session;
           return null;
        }
      }

    #endregion

    #region Pub

      public virtual LockSession MakeSession(string path, object shardingID, string description = null, int? maxAgeSec = null)
      {
        var session =  new LockSession(this, path, shardingID, description, maxAgeSec);
        m_Sessions[ session.ID ] = session;

        return session;
      }

      public LockTransactionResult ExecuteLockTransaction(LockSession session, LockTransaction transaction)
      {
         if (!Running) return LockTransactionResult.CallFailed;

         if (session==null || transaction==null)
          throw new LockingException(StringConsts.ARGUMENT_ERROR+GetType().Name+".ExecuteLockTransaction(session|tran==null)");

         checkSessionExists(session);

         if (m_InstrumentationEnabled)
           m_Stats.IncrementLong(session.Path);

         return DoExecuteLockTransaction(session, transaction);
      }

      public Task<LockTransactionResult> ExecuteLockTransactionAsync(LockSession session, LockTransaction transaction)
      {
        if (!Running) return Task.FromResult(LockTransactionResult.CallFailed);

        if (session==null || transaction==null)
          throw new LockingException(StringConsts.ARGUMENT_ERROR+GetType().Name+".ExecuteLockTransactionAsync(session|tran==null)");

        checkSessionExists(session);

        if (m_InstrumentationEnabled)
           m_Stats.IncrementLong(session.Path);

        return DoExecuteLockTransactionAsync(session, transaction);
      }


      public bool EndLockSession(LockSession session)
      {
        if (!Running) return false;

        if (session==null)
          throw new LockingException(StringConsts.ARGUMENT_ERROR+GetType().Name+".EndLockSession(session==null)");

        checkSessionExists(session);

        var ended = DoEndLockSession(session);

        LockSession d;
        m_Sessions.TryRemove(session.ID, out d);

        return ended;
      }

      public Task<bool> EndLockSessionAsync(LockSession session)
      {
        if (!Running) return Task.FromResult(false);

        if (session==null)
          throw new LockingException(StringConsts.ARGUMENT_ERROR+GetType().Name+".EndLockSessionAsync(session==null)");

        checkSessionExists(session);

        LockSession d;
        m_Sessions.TryRemove(session.ID, out d);

        return DoEndLockSessionAsync(session);
      }

    #endregion


    #region Protected

          protected abstract LockTransactionResult DoExecuteLockTransaction(LockSession session, LockTransaction transaction);
          protected abstract Task<LockTransactionResult> DoExecuteLockTransactionAsync(LockSession session, LockTransaction transaction);
          protected abstract bool DoEndLockSession(LockSession session);
          protected abstract Task<bool> DoEndLockSessionAsync(LockSession session);


          protected override void DoStart()
          {
            base.DoStart();
          }

          protected override void DoWaitForCompleteStop()
          {
            base.DoWaitForCompleteStop();
          }

          protected override void DoAcceptManagerVisit(object manager, DateTime managerNow)
          {
            dumpStats();
          }

    #endregion


    #region .pvt

      private void checkSessionExists(LockSession session)
      {
        var s = this[session.ID];
        if (s==null) throw new LockingException(StringConsts.LOCK_SESSION_NOT_ACTIVE_ERROR.Args(session.ID, session.Description));
      }

      private void dumpStats()
      {
        var instr = App.Instrumentation;
        if (!instr.Enabled) return;


        instr.Record( new Instrumentation.LockSessions( m_Sessions.Count ) );

        foreach( var kvp in m_Stats.SnapshotAllLongs(0))
          instr.Record( new Instrumentation.LockTransactionRequests(kvp.Key, kvp.Value) );
      }

    #endregion


  }

}
