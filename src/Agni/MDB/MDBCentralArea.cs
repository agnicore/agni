using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX;
using NFX.ApplicationModel;
using NFX.Environment;
using NFX.DataAccess.CRUD;
using NFX.DataAccess.Distributed;

namespace Agni.MDB
{
  /// <summary>
  /// Represents a single central area that has one central partition
  /// </summary>
  public sealed class MDBCentralArea : MDBArea
  {

    internal MDBCentralArea(MDBDataStore store, IConfigSectionNode node) : base(store, node)
    {
      m_CentralPartition = new Partition(this, node);
    }

    //Central has only one partition
    private Partition m_CentralPartition;


    public override string Name{ get{ return CENTRAL_AREA_NAME;}}

    /// <summary>
    /// Returns a single partition of MDBCentralArea
    /// </summary>
    public Partition CentralPartition{ get{ return m_CentralPartition;}}

    public override IEnumerable<Partition> AllPartitions { get { yield return m_CentralPartition;} }

    public override IEnumerable<Partition.Shard> AllShards { get { return m_CentralPartition.Shards;}}


    /// <summary>
    /// Returns CRUDOperations facade connected to the appropriate database server within the CENTRAL area's partition
    ///  which services the shard computed from sharding id
    /// </summary>
    public CRUDOperations ShardedOperationsFor(object idSharding)
    {
      return m_CentralPartition.ShardedOperationsFor(idSharding);
    }

  }

}
