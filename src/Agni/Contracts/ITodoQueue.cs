using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Glue;
using NFX.Log;

using Agni.Workers;

namespace Agni.Contracts
{

  /// <summary>
  /// Sends todos to the queue on the remote host
  /// </summary>
  [Glued]
  [LifeCycle(ServerInstanceMode.Singleton)]
  public interface ITodoQueue : IAgniService
  {
    /// <summary>
    /// Enqueues todos in the order. Returns the maximum size of the next call to Enqueue()
    /// which reflects the ability of the remote queue to enqueue more work.
    /// This may be used for dynamic flow control.
    /// Calling this method with empty or null array just returns the status
    /// </summary>
    int Enqueue(TodoFrame[] todos);
  }


  /// <summary>
  /// Contract for client of ITodoQueue svc
  /// </summary>
  public interface ITodoQueueClient : IAgniServiceClient, ITodoQueue
  {
    CallSlot Async_Enqueue(TodoFrame[] todos);
  }
}