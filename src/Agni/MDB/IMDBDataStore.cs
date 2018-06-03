using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX;
using NFX.DataAccess;
using NFX.DataAccess.CRUD;
using NFX.DataAccess.Distributed;
using NFX.ApplicationModel.Pile;


namespace Agni.MDB
{
  /// <summary>
  /// Stipulates a contract for MDBDataStore
  /// </summary>
  public interface IMDBDataStore  : IDataStore
  {
    string SchemaName{ get;}
    string BankName{ get;}

    IGDIDProvider GDIDGenerator{ get;}

    MDBCentralArea CentralArea{ get;}

    IRegistry<MDBArea> Areas{ get;}

    /// <summary>
    /// Pile big memory cache
    /// </summary>
    ICache Cache { get;}

    /// <summary>
    /// Returns CRUDOperations facade connected to the appropriate database server within the named area
    ///  which services the shard computed from the briefcase GDID
    /// </summary>
    CRUDOperations PartitionedOperationsFor(string areaName, GDID idBriefcase);

    /// <summary>
    /// Returns CRUDOperations facade connected to the appropriate shard within the central area as
    /// determined by the the shardingID
    /// </summary>
    CRUDOperations CentralOperationsFor(object shardingID);
  }
}
