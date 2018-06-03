using System.Collections.Generic;
using NFX.Glue;
using NFX.DataAccess.Distributed;

using Agni.Contracts;

namespace Agni.Social.Graph
{
  /// <summary>
  /// Handles social graph functionality dealing with event subscription and broadcasting
  /// </summary>
  [Glued]
  [LifeCycle(ServerInstanceMode.Singleton)]
  public interface IGraphEventSystem : IAgniService
  {
    /// <summary>
    /// Emits the event - notifies all subscribers (watchers, friends etc.) about the event.
    /// The physical notification happens via IGraphHost implementation
    /// </summary>
    void EmitEvent(Event evt);

    /// <summary>
    /// Subscribes recipient node to the emitter node. Unlike friends the susbscription connection is uni-directional
    /// </summary>
    void Subscribe(GDID gRecipientNode, GDID gEmitterNode, byte[] parameters);

    /// <summary>
    /// Removes the subscription. Unlike friends the subscription connection is uni-directional
    /// </summary>
    void Unsubscribe(GDID gRecipientNode, GDID gEmitterNode);

    /// <summary>
    /// Returns an estimated approximate number of subscribers that an emitter has
    /// </summary>
    long EstimateSubscriberCount(GDID gEmitterNode);

    /// <summary>
    /// Returns Subscribers for Emitter from start position
    /// </summary>
    IEnumerable<GraphNode> GetSubscribers(GDID gEmitterNode, long start, int count);
  }

  /// <summary>
  /// Contract for client of IGraphEventSystem svc
  /// </summary>
  public interface IGraphEventSystemClient : IAgniServiceClient, IGraphEventSystem
  {
    //todo Add async versions
  }
}
