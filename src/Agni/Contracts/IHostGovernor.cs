using System;

using NFX;
using NFX.Glue;

namespace Agni.Contracts
{
  /// <summary>
  /// Returns information about the host
  /// </summary>
  [Glued]
  [LifeCycle(ServerInstanceMode.Singleton)]
  public interface IHostGovernor : IAgniService
  {
    HostInfo GetHostInfo();
  }


  /// <summary>
  /// Contract for client of IPinger svc
  /// </summary>
  public interface IHostGovernorClient : IAgniServiceClient, IHostGovernor
  {
    CallSlot Async_GetHostInfo();
  }
}