﻿using System;
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
  /// Represents a general ancestor for CENTRAL or partitioned areas
  /// </summary>
  public abstract partial class MDBArea : MDBAppComponent, INamed
  {
    #region CONSTS
      public const string CENTRAL_AREA_NAME = "CENTRAL";

      public const string CONFIG_AREA_SECTION = "area";
      public const string CONFIG_PARTITION_SECTION = "partition";
      public const string CONFIG_SHARD_SECTION = "shard";
      public const string CONFIG_START_GDID_ATTR = "start-gdid";
      public const string CONFIG_ORDER_ATTR = "order";

      public const string CONFIG_PRIMARY_CONNECT_STRING_ATTR = "primary-cs";
      public const string CONFIG_SECONDARY_CONNECT_STRING_ATTR = "secondary-cs";
    #endregion

    #region .ctor
      protected MDBArea(MDBDataStore store, IConfigSectionNode node) : base(store)
      {
        if (store==null || node==null || !node.Exists)
          throw new MDBException(StringConsts.ARGUMENT_ERROR+"MDBArea.ctor(store==null|node==null|!Exists)");


        ConfigAttribute.Apply(this, node);

        var dsnode = node[CommonApplicationLogic.CONFIG_DATA_STORE_SECTION];
        if (!dsnode.Exists)
          throw new MDBException(StringConsts.MDB_AREA_CONFIG_NO_DATASTORE_ERROR.Args(node.RootPath));

        m_PhysicalDataStore = FactoryUtils.MakeAndConfigure<ICRUDDataStoreImplementation>(dsnode, args: new []{ this });
      }


      protected override void Destructor()
      {
        DisposableObject.DisposeAndNull(ref m_PhysicalDataStore);
        base.Destructor();
      }
    #endregion

    #region Fields
      private ICRUDDataStoreImplementation m_PhysicalDataStore;
    #endregion

    #region Properties

      public MDBDataStore Store{ get { return (MDBDataStore)base.ComponentDirector;}}
      public abstract string Name{ get;}

      /// <summary>
      /// Physical data store that services the area
      /// </summary>
      public ICRUDDataStoreImplementation PhysicalDataStore
      {
        get { return m_PhysicalDataStore;}
      }

      /// <summary>
      /// Returns all ordered partitions of the area - one for central, or all actual partitions for partitioned area
      /// </summary>
      public abstract IEnumerable<Partition> AllPartitions { get;}

      /// <summary>
      /// Returns all ordered shards within ordered partitions
      /// </summary>
      public abstract IEnumerable<Partition.Shard> AllShards { get;}

    #endregion
  }

}
