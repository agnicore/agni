using System;
using System.Collections.Generic;

using NFX.Glue;
using NFX.DataAccess.Distributed;

using Agni.Contracts;

namespace Agni.Social.Graph
{
  /// <summary>
  /// Handles the base social graph functionality such as CRUD of graph nodes (users, forums, groups etc..)
  /// </summary>
  [Glued]
  public interface IGraphNodeSystem : IAgniService
  {
    /// <summary>
    /// Saves the GraphNode instances into the system.
    /// If a node with such ID already exists, updates it, otherwise creates a new node
    /// Return GDID Node
    /// </summary>
    GraphChangeStatus SaveNode(GraphNode node);

    /// <summary>
    /// Fetches the GraphNode by its unique GDID or unassigned node if not found
    /// </summary>
    GraphNode GetNode(GDID gNode);

    /// <summary>
    /// Deletes node by GDID
    /// </summary>
    GraphChangeStatus DeleteNode(GDID gNode);

    /// <summary>
    /// Undeletes node by GDID
    /// </summary>
    GraphChangeStatus UndeleteNode(GDID gNode);

    /// <summary>
    /// Physically removes node by GDID from database
    /// </summary>
    GraphChangeStatus RemoveNode(GDID gNode);
  }

  /// <summary>
  /// Contract for client of IGraphSystem svc
  /// </summary>
  public interface IGraphNodeSystemClient : IAgniServiceClient, IGraphNodeSystem
  {
    //todo Add async versions
  }
}
