using System;

using NFX;
using NFX.DataAccess.CRUD;

using Agni.Workers;
using NFX.Environment;

namespace Agni.Demo.Todos
{
  [TodoQueue("TodoQueue", "94EFBB80-3CF4-4393-A04B-75A8E94547C0")]
  public class FinishDemoProcessTodo : CorrelatedTodo
  {
    [Config("$pid")] [Field(backendName: "pid")] public string PIDStr { get; set; }

    public PID PID { get { return PID.Parse(PIDStr); } set { PIDStr = value.ToString(); } }

    protected override void DoPrepareForEnqueuePreValidate(string targetName)
    {
      SysShardingKey = PID.ID;
      SysParallelKey = PID.ID;
      SysCorrelationKey = "{0}-{1}".Args(GetType().Name, PID.ID);
      base.DoPrepareForEnqueuePreValidate(targetName);
    }

    protected override MergeResult Merge(ITodoHost host, DateTime utcNow, CorrelatedTodo another)
    {
      var anotherTodo = another as FinishDemoProcessTodo;
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
      var result = FinishSignal.Dispatch(PID, "OK");
      var ok = result is OkSignal;
      return ok ? ExecuteState.Complete : ExecuteState.ReexecuteAfterError;
    }
  }
}
