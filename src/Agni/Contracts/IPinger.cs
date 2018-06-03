using System;

using NFX;
using NFX.Glue;

namespace Agni.Contracts
{
  /// <summary>
  /// Used to see if the host responds at all
  /// </summary>
  [Glued]
  [LifeCycle(ServerInstanceMode.Singleton)]
  public interface IPinger : IAgniService
  {
    void Ping();
  }


  /// <summary>
  /// Contract for client of IPinger svc
  /// </summary>
  public interface IPingerClient : IAgniServiceClient, IPinger
  {
    CallSlot Async_Ping();
  }
}