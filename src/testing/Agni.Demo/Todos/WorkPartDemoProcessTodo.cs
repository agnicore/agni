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
  [TodoQueue("TodoQueue", "CC9803EE-B991-4206-AD24-951A51E722FB")]
  public class WorkPartDemoProcessTodo : Todo
  {
    private static readonly ExecuteState State_WorkPart = ExecuteState.Initial;
    private static readonly ExecuteState State_DispatchDone = new ExecuteState(1);
    private static readonly ExecuteState State_EnqueueFinish = new ExecuteState(2);

    [Config("$pid")] [Field(backendName: "pid")] public string PIDStr { get; set; }
    [Config("$p")]   [Field(backendName: "p")]   public int    Part { get; set; }
                     [Field(backendName: "d")]   public bool   Done { get; set; }

    public PID PID { get { return PID.Parse(PIDStr); } set { PIDStr = value.ToString(); } }

    protected override void DoPrepareForEnqueuePreValidate(string targetName)
    {
      SysShardingKey = "{0}-{1}".Args(Part, PID.ID);
      base.DoPrepareForEnqueuePreValidate(targetName);
    }

    protected override ExecuteState Execute(ITodoHost host, DateTime utcBatchNow)
    {
      //-------------------------------------------------------------------------------
      if (SysState == State_WorkPart)
      {
        if (Done) return State_DispatchDone;
        var rndMs = ExternalRandomGenerator.Instance.NextScaledRandomInteger(3* 60 * 1000, 5 * 60 * 1000);
        SysStartDate = utcBatchNow.AddMilliseconds(rndMs);
        Done = true;
        return ExecuteState.ReexecuteUpdated;
      }

      //-------------------------------------------------------------------------------
      if (SysState == State_DispatchDone)
      {
        try
        {
          var signal = WorkPartDoneSignal.Make(PID, Part, "OK");
          var result = AgniSystem.ProcessManager.Dispatch(signal);
          if (result is OkSignal)
            return State_EnqueueFinish;
        }
        catch (Exception error)
        {
          host.Log(MessageType.Error, this, "Execute().DispatchDone", error.ToMessageWithType(), error);
          return ExecuteState.ReexecuteAfterError;
        }
        throw new DemoException("Wrong result signal");
      }

      //-------------------------------------------------------------------------------
      if (SysState == State_EnqueueFinish)
      {
        try
        {
          var todo = Todo.MakeNew<FinishDemoProcessTodo>();
          todo.PID = PID;
          todo.SysStartDate = utcBatchNow.AddMinutes(5);
          AgniSystem.ProcessManager.Enqueue(todo, "todo", "todoqueue");
          return ExecuteState.Complete;
        }
        catch (Exception error)
        {
          host.Log(MessageType.Error, this, "Execute().EnqueueFinish", error.ToMessageWithType(), error);
          return ExecuteState.ReexecuteAfterError;
        }
      }

      throw new WorkersException(StringConsts.UNKNOWN_TODO_STATE_ERROR.Args(GetType().Name, SysState));
    }
  }
}
