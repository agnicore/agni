﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX;
using NFX.DataAccess.CRUD;
using NFX.Environment;

namespace Agni.Workers
{
  [Signal("0626D816-FCF6-40BD-8652-274CAEEA3E63")]
  public sealed class OkSignal : ResultSignal
  {
    public static OkSignal Make(Process process)
    {
      var ok = MakeNew<OkSignal>(process);
      return ok;
    }
  }
  [Signal("C3FFB82B-5C9D-4683-A252-1FF6C857F5ED")]
  public sealed class UnknownSignal : ResultSignal
  {
    public static UnknownSignal Make(Process process, Signal signal)
    {
      var unknown = MakeNew<UnknownSignal>(process);
      unknown.SignalType = signal.SysTypeGuid;
      return unknown;
    }

    [Config][Field(backendName: "guid", required: true)] public Guid? SignalType { get; set; }
  }

  [Signal("DE276340-8075-490C-9FAF-F9240480902A")] public sealed class CancelSignal : Signal { }
  [Signal("5ED96CF6-763E-4567-AB98-E705F5501264")] public sealed class TerminateSignal : Signal { }
  [Signal("D0E26B05-CDC0-4A2A-8CFD-7F3D0A39C8C4")] public sealed class FinishSignal : Signal
  {
    public static ResultSignal Dispatch(PID pid, string description = null)
    {
      var finish = MakeNew<FinishSignal>(pid);
      finish.Description = description;
      return AgniSystem.ProcessManager.Dispatch(finish);
    }

    [Config][Field(backendName: "d")] public string Description { get; set; }
  }
  [Signal("27DE1FF6-98FE-418D-8243-EE991510D84E")]
  public sealed class FinalizeSignal : Signal
  {
    public static ResultSignal Dispatch(PID pid)
    {
      var finalize = MakeNew<FinalizeSignal>(pid);
      return AgniSystem.ProcessManager.Dispatch(finalize);
    }
  }
}
