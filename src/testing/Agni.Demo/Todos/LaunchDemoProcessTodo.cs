using System;

using NFX;
using NFX.DataAccess.CRUD;
using NFX.Log;

using Agni.Workers;
using Agni.Demo.Processes;
using Agni.Coordination;
using NFX.Environment;

namespace Agni.Demo.Todos
{
  [TodoQueue("TodoQueue", "4B20C50C-6D1F-4196-B8C2-9657C5760233")]
  public class LaunchDemoProcessTodo : CorrelatedTodo
  {
    public const int DEFAULT_PART_COUNT = 16;

    private static readonly ExecuteState State_SpawnProcess = ExecuteState.Initial;
    private static readonly ExecuteState State_EnqueueWorkParts = new ExecuteState(1);

    [Config("$pid")] [Field(backendName: "pid")] public string PIDStr { get; set; }
    [Config("$pc")]  [Field(backendName: "pc")]  public int PartCount { get; set; }

    public PID PID { get { return PID.Parse(PIDStr); } set { PIDStr = value.ToString(); } }

    private bool m_IsModified;

    protected override void DoPrepareForEnqueuePreValidate(string targetName)
    {
      SysShardingKey = PID.ID;
      SysParallelKey = PID.ID;
      SysCorrelationKey = "{0}-{1}".Args(GetType().Name, PID.ID);
      base.DoPrepareForEnqueuePreValidate(targetName);
    }

    protected override MergeResult Merge(ITodoHost host, DateTime utcNow, CorrelatedTodo another)
    {
      var anotherTodo = another as LaunchDemoProcessTodo;
      if (anotherTodo == null) return MergeResult.None;

      if (SysStartDate < anotherTodo.SysStartDate)
      {
        SysStartDate = anotherTodo.SysStartDate;
        return MergeResult.Merged;
      }
      return MergeResult.IgnoreAnother;
    }

    protected override ExecuteState Execute(ITodoHost host, DateTime utcBatchNow)
    {
      //-------------------------------------------------------------------------------
      if (SysState == State_SpawnProcess)
      {
        try
        {
          DemoProcess.Spawn(PID, PartCount);
          return State_EnqueueWorkParts;
        }
        catch (Exception error)
        {
          host.Log(MessageType.Error, this, "Execute().SpawnProcess", error.ToMessageWithType(), error);
          return ExecuteState.ReexecuteAfterError;
        }
      }

      //-------------------------------------------------------------------------------
      if (SysState == State_EnqueueWorkParts)
      {
        try
        {
          var todo = MakeNew<WorkPartDemoProcessTodo>();
          todo.PID = PID;
          PartCount--;
          m_IsModified = true;
          todo.Part = PartCount;
          AgniSystem.ProcessManager.Enqueue(todo, "todo", "todoqueue");
          return PartCount == 0 ? ExecuteState.Complete : State_EnqueueWorkParts;
        }
        catch (Exception error)
        {
          host.Log(MessageType.Error, this, "Execute().EnqueueWorkParts", error.ToMessageWithType(), error);
          return m_IsModified ? ExecuteState.ReexecuteUpdatedAfterError : ExecuteState.ReexecuteAfterError;
        }
      }

      throw new WorkersException(StringConsts.UNKNOWN_TODO_STATE_ERROR.Args(GetType().Name, SysState));
    }
  }
}
