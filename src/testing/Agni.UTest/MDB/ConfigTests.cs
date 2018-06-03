using Agni.MDB;
using NFX;
using NFX.ApplicationModel;
using NFX.DataAccess.Distributed;
using NFX.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agni.UTest.MDB
{
  [Runnable]
  public class ConfigTests : BaseTestRigWithMetabase
  {

      //private Agni.Locking.Server.LockServerService m_Server;


      //protected override void DoRigSetup()
      //{
      //  m_Server = new Agni.Locking.Server.LockServerService(null);
      //  m_Server.Start();
      //}

      //protected override void DoRigTearDown()
      //{
      //  m_Server.Dispose();
      //}


      public const string  CONFIG_1 = @"
app
{
  data-store
  {
    type='Agni.MDB.MDBDataStore, Agni'

    target-name='MDB'
    schema-name='schema1'
    bank-name='bank1'

    area
    {
       name='central'
       data-store
       {
         type='NFX.DataAccess.MySQL.MySQLDataStore, NFX.MySQL'
         script-assembly='Agni.UTest'
       }

       shard{ order=0 primary-cs='cs0.1' secondary-cs='cs0.2' }
       shard{ order=1 primary-cs='cs1.1' secondary-cs='cs1.2' }
    }

    area
    {
       name='user'
       data-store
       {
         type='NFX.DataAccess.MySQL.MySQLDataStore, NFX.MySQL'
         script-assembly='Agni.UTest'
       }
       partition
       {
         start-gdid='0:0:0'
         shard{ order=0 primary-cs='p0-cs0.1' secondary-cs='p0-cs0.2' }
         shard{ order=1 primary-cs='p0-cs1.1' secondary-cs='p0-cs1.2' }
         shard{ order=2 primary-cs='p0-cs2.1' secondary-cs='p0-cs2.2' }
       }

       partition
       {
         start-gdid='0:0:250000'
         shard{ order=0 primary-cs='p1-cs0.1' secondary-cs='p1-cs0.2' }
         shard{ order=1 primary-cs='p1-cs1.1' secondary-cs='p1-cs1.2' }
       }

       partition
       {
         start-gdid='0:0:750000'
         shard{ order=0 primary-cs='p2-cs0.1' secondary-cs='p2-cs0.2' }
         shard{ order=1 primary-cs='p2-cs1.1' secondary-cs='p2-cs1.2' }
         shard{ order=2 primary-cs='p2-cs2.1' secondary-cs='p2-cs2.2' }
         shard{ order=3 primary-cs='p2-cs3.1' secondary-cs='p2-cs3.2' }
       }
    }

  }
}";


      [Run]
      public void CFG_MountMDBStore()
      {
          using(var app = new ServiceBaseApplication(null, CONFIG_1.AsLaconicConfig(handling: ConvertErrorHandling.Throw)))
          {
            var ds = app.DataStore as MDBDataStore;
            Aver.IsNotNull( ds );

            Aver.AreEqual(2,  ds.Areas.Count);
            Aver.IsTrue( object.ReferenceEquals(ds.CentralArea, ds.Areas[MDBArea.CENTRAL_AREA_NAME]) );

            var auser = ds.Areas["User"] as MDBPartitionedArea;
            Aver.IsNotNull( auser );
            Aver.AreEqual(3,  auser.Partitions.Count() );

          }
      }//test

      [Run]
      public void CFG_PartitionedRouting()
      {
          using(var app = new ServiceBaseApplication(null, CONFIG_1.AsLaconicConfig(handling: ConvertErrorHandling.Throw)))
          {
            var ds = app.DataStore as MDBDataStore;
            Aver.IsNotNull( ds );

            //PARTITION with Authority
            var ops = ds.PartitionedOperationsFor("user", new GDID(0, 1, 250000));
            Aver.AreEqual(new GDID(0, 0, 250000), ops.Shard.Partition.StartGDID);
            Aver.IsTrue( object.ReferenceEquals(ops.Shard.Area, ds.Areas["user"]));

            ops = ds.PartitionedOperationsFor("user", new GDID(0, 1, 250022));
            Aver.AreEqual(new GDID(0, 0, 250000), ops.Shard.Partition.StartGDID);
            Aver.IsTrue( object.ReferenceEquals(ops.Shard.Area, ds.Areas["user"]));

            ops = ds.PartitionedOperationsFor("user", new GDID(0, 15, 250022));
            Aver.AreEqual(new GDID(0, 0, 250000), ops.Shard.Partition.StartGDID);
            Aver.IsTrue( object.ReferenceEquals(ops.Shard.Area, ds.Areas["user"]));

            ops = ds.PartitionedOperationsFor("user", new GDID(0, 1, 123));
            Aver.AreEqual(new GDID(0, 0, 0), ops.Shard.Partition.StartGDID);
            Aver.IsTrue( object.ReferenceEquals(ops.Shard.Area, ds.Areas["user"]));

            ops = ds.PartitionedOperationsFor("user", new GDID(0, 9, 123));
            Aver.AreEqual(new GDID(0, 0, 0), ops.Shard.Partition.StartGDID);
            Aver.IsTrue( object.ReferenceEquals(ops.Shard.Area, ds.Areas["user"]));


            //all huge numbers get dropped at the last partition
            ops = ds.PartitionedOperationsFor("user", new GDID(0, 7, 4298379847293747));
            Aver.AreEqual(new GDID(0, 0, 750000), ops.Shard.Partition.StartGDID);
            Aver.IsTrue( object.ReferenceEquals(ops.Shard.Area, ds.Areas["user"]));

            ops = ds.PartitionedOperationsFor("user", new GDID(123, 7, 4298379847293747));
            Aver.AreEqual(new GDID(0, 0, 750000), ops.Shard.Partition.StartGDID);
            Aver.IsTrue( object.ReferenceEquals(ops.Shard.Area, ds.Areas["user"]));



            //PARTITION 0
            ops = ds.PartitionedOperationsFor("user", new GDID(0, 0, 0));
            Aver.AreEqual("p0-cs0.1", ops.Shard.EffectiveConnectionString);
            Aver.AreEqual(0, ops.Shard.Order);
            Aver.IsTrue( object.ReferenceEquals(ops.Shard.Area, ds.Areas["user"]));

            ops = ds.PartitionedOperationsFor("user", new GDID(0, 0, 1));
            Aver.AreEqual("p0-cs1.1", ops.Shard.EffectiveConnectionString);
            Aver.AreEqual(1, ops.Shard.Order);
            Aver.IsTrue( object.ReferenceEquals(ops.Shard.Area, ds.Areas["user"]));

            ops = ds.PartitionedOperationsFor("user", new GDID(0, 0, 2));
            Aver.AreEqual("p0-cs2.1", ops.Shard.EffectiveConnectionString);
            Aver.AreEqual(2, ops.Shard.Order);
            Aver.IsTrue( object.ReferenceEquals(ops.Shard.Area, ds.Areas["user"]));

            ops = ds.PartitionedOperationsFor("user", new GDID(0, 0, 3));
            Aver.AreEqual("p0-cs0.1", ops.Shard.EffectiveConnectionString);
            Aver.AreEqual(0, ops.Shard.Order);
            Aver.IsTrue( object.ReferenceEquals(ops.Shard.Area, ds.Areas["user"]));

            //PARTITION 1
            ops = ds.PartitionedOperationsFor("user", new GDID(0, 0, 250000));
            Aver.AreEqual("p1-cs0.1", ops.Shard.EffectiveConnectionString);
            Aver.AreEqual(0, ops.Shard.Order);
            Aver.IsTrue( object.ReferenceEquals(ops.Shard.Area, ds.Areas["user"]));

            ops = ds.PartitionedOperationsFor("user", new GDID(0, 0, 250001));
            Aver.AreEqual("p1-cs1.1", ops.Shard.EffectiveConnectionString);
            Aver.AreEqual(1, ops.Shard.Order);
            Aver.IsTrue( object.ReferenceEquals(ops.Shard.Area, ds.Areas["user"]));

            ops = ds.PartitionedOperationsFor("user", new GDID(0, 0, 250002));
            Aver.AreEqual("p1-cs0.1", ops.Shard.EffectiveConnectionString);
            Aver.AreEqual(0, ops.Shard.Order);
            Aver.IsTrue( object.ReferenceEquals(ops.Shard.Area, ds.Areas["user"]));

            //PARTITION 2
            ops = ds.PartitionedOperationsFor("user", new GDID(0, 0, 750000));
            Aver.AreEqual("p2-cs0.1", ops.Shard.EffectiveConnectionString);
            Aver.AreEqual(0, ops.Shard.Order);
            Aver.IsTrue( object.ReferenceEquals(ops.Shard.Area, ds.Areas["user"]));

            ops = ds.PartitionedOperationsFor("user", new GDID(0, 0, 750001));
            Aver.AreEqual("p2-cs1.1", ops.Shard.EffectiveConnectionString);
            Aver.AreEqual(1, ops.Shard.Order);
            Aver.IsTrue( object.ReferenceEquals(ops.Shard.Area, ds.Areas["user"]));

            ops = ds.PartitionedOperationsFor("user", new GDID(0, 0, 750002));
            Aver.AreEqual("p2-cs2.1", ops.Shard.EffectiveConnectionString);
            Aver.AreEqual(2, ops.Shard.Order);
            Aver.IsTrue( object.ReferenceEquals(ops.Shard.Area, ds.Areas["user"]));

            ops = ds.PartitionedOperationsFor("user", new GDID(0, 0, 750003));
            Aver.AreEqual("p2-cs3.1", ops.Shard.EffectiveConnectionString);
            Aver.AreEqual(3, ops.Shard.Order);
            Aver.IsTrue( object.ReferenceEquals(ops.Shard.Area, ds.Areas["user"]));

            ops = ds.PartitionedOperationsFor("user", new GDID(0, 0, 750004));
            Aver.AreEqual("p2-cs0.1", ops.Shard.EffectiveConnectionString);
            Aver.AreEqual(0, ops.Shard.Order);
            Aver.IsTrue( object.ReferenceEquals(ops.Shard.Area, ds.Areas["user"]));
          }
      }//test

      [Run]
      public void CFG_Area_GetPartitionsStartingFromBriefcase()
      {
          using(var app = new ServiceBaseApplication(null, CONFIG_1.AsLaconicConfig(handling: ConvertErrorHandling.Throw)))
          {
            var ds = app.DataStore as MDBDataStore;
            Aver.IsNotNull( ds );

            var area = ds.Areas["user"] as MDBPartitionedArea;
            Aver.IsNotNull( area );

            var partitions = area.GetPartitionsStartingFromBriefcase(new GDID(0, 0, 250000)).ToList();
            Aver.AreEqual(2, partitions.Count);
            Aver.AreEqual(new GDID(0, 0, 250000), partitions[0].StartGDID);
            Aver.AreEqual(new GDID(0, 0, 750000), partitions[1].StartGDID);

            partitions = area.GetPartitionsStartingFromBriefcase(new GDID(0, 0, 250001)).ToList();
            Aver.AreEqual(2, partitions.Count);
            Aver.AreEqual(new GDID(0, 0, 250000), partitions[0].StartGDID);
            Aver.AreEqual(new GDID(0, 0, 750000), partitions[1].StartGDID);

            partitions = area.GetPartitionsStartingFromBriefcase(new GDID(0, 0, 1000000)).ToList();
            Aver.AreEqual(1, partitions.Count);
            Aver.AreEqual(new GDID(0, 0, 750000), partitions[0].StartGDID);

            partitions = area.GetPartitionsStartingFromBriefcase(null).ToList();
            Aver.AreEqual(3, partitions.Count);
            Aver.AreEqual(GDID.Zero, partitions[0].StartGDID);
            Aver.AreEqual(new GDID(0, 0, 250000), partitions[1].StartGDID);
            Aver.AreEqual(new GDID(0, 0, 750000), partitions[2].StartGDID);
          }
      }
  }
}
