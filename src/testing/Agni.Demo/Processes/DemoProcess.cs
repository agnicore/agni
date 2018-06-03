using System;

using NFX;
using NFX.DataAccess.CRUD;

using Agni.Workers;
using NFX.Environment;

namespace Agni.Demo.Processes
{
  [Signal("C4636945-6391-46D9-9510-52405C24DC64")]
  public class WorkPartDoneSignal : Signal
  {
    public static WorkPartDoneSignal Make(PID pid, int part, string message)
    {
      var workPartDone = MakeNew<WorkPartDoneSignal>(pid);
      workPartDone.Part = part;
      workPartDone.Message = message;
      return workPartDone;
    }

    [Field(backendName: "p")] public int Part { get; set; }
    [Field(backendName: "m")] public string Message { get; set; }
  }

  [Process("C02FAD1B-0516-4A53-9A01-3B15F45CC451", Description = "Demo Process")]
  public class DemoProcess : Process
  {
    public static DemoProcess Spawn(PID pid, int partCount)
    {
      var process = MakeNew<DemoProcess>(pid);
      process.PartCount = partCount;
      process.Messages = new string[partCount];
      AgniSystem.ProcessManager.Spawn(process);
      return process;
    }

    [Config("$pc")]
    [Field(backendName: "pc")]
    public int PartCount { get; set; }
    [Field(backendName: "ms")]
    public string[] Messages { get; set; }

    protected override ResultSignal DoAccept(IProcessHost host, Signal signal)
    {
      if (signal is WorkPartDoneSignal)
      {
        var workPartDone = signal as WorkPartDoneSignal;
        Messages[workPartDone.Part] = "{0} - '{1}' - {2}".Args(workPartDone.Part, workPartDone.Message, App.TimeSource.UTCNow);
        host.Update(this);
        return OkSignal.Make(this);
      }

      return null;
    }

    protected override void Merge(IProcessHost host, DateTime utcNow, Process another) {}
  }
}
