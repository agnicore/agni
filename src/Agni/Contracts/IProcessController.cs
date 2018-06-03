using System;
using System.Collections.Generic;

using NFX.Glue;

using Agni.Workers;

namespace Agni.Contracts
{

  /// <summary>
  /// Controls the spawning and execution of processes.
  /// Dispatches signals
  /// </summary>
  [Glued]
  [LifeCycle(ServerInstanceMode.Singleton)]
  public interface IProcessController : IAgniService
  {
    void Spawn(ProcessFrame frame);

    SignalFrame Dispatch(SignalFrame signal);

    ProcessFrame Get(PID pid);

    ProcessDescriptor GetDescriptor(PID pid);

    IEnumerable<ProcessDescriptor> List(int processorID);
  }


  /// <summary>
  /// Contract for client of ITodoQueue svc
  /// </summary>
  public interface IProcessControllerClient : IAgniServiceClient, IProcessController
  {
    CallSlot Async_Spawn(ProcessFrame frame);
    CallSlot Async_Dispatch(SignalFrame signal);

    CallSlot Async_Get(PID pid);
    CallSlot Async_GetDescriptor(PID pid);
    CallSlot Async_List(int processorID);
  }
}