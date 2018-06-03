using System;
using System.Diagnostics;
using System.Threading.Tasks;

using NFX;
using NFX.ApplicationModel;
using NFX.DataAccess.CRUD;
using NFX.DataAccess.Distributed;
using NFX.DataAccess.MongoDB.Connector;
using NFX.Scripting;

using Agni.KDB;

namespace Agni.UTest.KDB
{
  [Runnable]
  public class DefaultKDBStoretests : BaseTestRigWithMetabase
  {
      public const string  CONFIG_1 = @"
app
{
  data-store
  {
    type='Agni.KDB.DefaultKDBStore, Agni.MongoDB'

    shard
    {
        order=0
        primary-cs='mongo{server=""mongo://localhost:27017"" db=""UT_ACKDB0""}'
        secondary-cs='mongo{server=""mongo://localhost:27017"" db=""UT_ACKDB0""}'
    }

    shard
    {
        order=1
        primary-cs='mongo{server=""mongo://localhost:27017"" db=""UT_ACKDB1""}'
        secondary-cs='mongo{server=""mongo://localhost:27017"" db=""UT_ACKDB1""}'
    }
  }
}";

      public const string CONFIG_FALLBACK_1 = @"
app
{
  data-store
  {
    type='Agni.KDB.DefaultKDBStore, Agni.MongoDB'

    shard
    {
        order=0
        primary-cs='mongo{server=""mongo://localhost:27017"" db=""UT_ACKDB0""}'
        secondary-cs='mongo{server=""mongo://localhost:27017"" db=""UT_ACKDB0""}'
    }
  }
}";

      public const string CONFIG_FALLBACK_2 = @"
app
{
  data-store
  {
    type='Agni.KDB.DefaultKDBStore, Agni.MongoDB'

    shard
    {
        order=0
        primary-cs='mongo{server=""mongo://localhost:27017"" db=""UT_ACKDB1""}'
        secondary-cs='mongo{server=""mongo://localhost:27017"" db=""UT_ACKDB1""}'
    }

    shard
    {
        order=1
        primary-cs='mongo{server=""mongo://localhost:27017"" db=""UT_ACKDB2""}'
        secondary-cs='mongo{server=""mongo://localhost:27017"" db=""UT_ACKDB2""}'
    }

    fallback
    {
      shard
      {
          order=0
          primary-cs='mongo{server=""mongo://localhost:27017"" db=""UT_ACKDB0""}'
          secondary-cs='mongo{server=""mongo://localhost:27017"" db=""UT_ACKDB0""}'
      }
    }
  }
}";

  public class TestRow: TypedRow
  {
    [Field(backendName: "n")] public string Name        {get;set;}
    [Field(backendName: "d")] public string Description {get;set;}
  }

  public class TwoGdidRow: TypedRow
  {
    [Field(backendName: "g1")] public GDID G1  {get; set;}
    [Field(backendName: "g2")] public GDID G2  {get; set;}
  }

  private void DropCollection(string collection)
  {
    var srv = MongoClient.Instance[new NFX.Glue.Node("mongo://localhost:27017")];
    var db0 = srv["UT_ACKDB0"];
    var db1 = srv["UT_ACKDB1"];
    var db2 = srv["UT_ACKDB2"];
    db0[collection].Drop();
    db1[collection].Drop();
    db2[collection].Drop();
  }

      [Run]
      public void KDB_T00000_MountKDBStore()
      {
          using(var app = new ServiceBaseApplication(null, CONFIG_1.AsLaconicConfig(handling: ConvertErrorHandling.Throw)))
          {
            var ds = app.DataStore as DefaultKDBStore;
            Aver.IsNotNull( ds );

            Aver.IsNotNull(ds.RootShardSet);
            Aver.AreEqual(2, ds.RootShardSet.Shards.Length);
            Aver.AreObjectsEqual(ds, ds.RootShardSet.Store);
            Aver.AreEqual(0, ds.RootShardSet.FallbackLevel);
            Aver.IsNull(ds.RootShardSet.Fallback);
            Aver.IsNull(ds.RootShardSet.FallbackParent);
            Aver.AreObjectsEqual(ds, ds.RootShardSet.Shards[0].Store);
            Aver.AreObjectsEqual(ds, ds.RootShardSet.Shards[1].Store);
            Aver.AreObjectsEqual(ds.RootShardSet, ds.RootShardSet.Shards[0].ShardSet);
            Aver.AreObjectsEqual(ds.RootShardSet, ds.RootShardSet.Shards[1].ShardSet);
            Aver.AreEqual(0, ds.RootShardSet.Shards[0].Order);
            Aver.AreEqual(1, ds.RootShardSet.Shards[1].Order);
            Aver.AreEqual("mongo{server=\"mongo://localhost:27017\" db=\"UT_ACKDB0\"}", ds.RootShardSet.Shards[0].PrimaryHostConnectString);
            Aver.AreEqual("mongo{server=\"mongo://localhost:27017\" db=\"UT_ACKDB0\"}", ds.RootShardSet.Shards[0].SecondaryHostConnectString);
            Aver.AreEqual("mongo{server=\"mongo://localhost:27017\" db=\"UT_ACKDB1\"}", ds.RootShardSet.Shards[1].PrimaryHostConnectString);
            Aver.AreEqual("mongo{server=\"mongo://localhost:27017\" db=\"UT_ACKDB1\"}", ds.RootShardSet.Shards[1].SecondaryHostConnectString);
            Aver.AreEqual(ds.RootShardSet.Shards[0].PrimaryHostConnectString, ds.RootShardSet.Shards[0].EffectiveConnectionString);
            Aver.AreEqual(ds.RootShardSet.Shards[1].PrimaryHostConnectString, ds.RootShardSet.Shards[1].EffectiveConnectionString);
          }
      }

      [Run]
      public void KDB_T00010_GetPutDeleteRow()
      {
        using(var app = new ServiceBaseApplication(null, CONFIG_1.AsLaconicConfig(handling: ConvertErrorHandling.Throw)))
        {
          var tbl = "tbla";
          DropCollection(tbl);

          var ds = app.DataStore as DefaultKDBStore;
          var key = "key1".ToUTF8Bytes();
          var r = ds.Get(tbl, key);
          Aver.IsNull(r);
          ds.Put(tbl, key, new TestRow {Name="Gagarin", Description="Garagin"});
          var result = ds.Get<TestRow>(tbl, key);
          Aver.IsTrue(result.IsAssigned);
          Aver.AreEqual("Gagarin", result.Value.Name);
          Aver.AreEqual("Garagin", result.Value.Description);
          Aver.AreEqual(-1, result.SlidingExpirationDays);
          Aver.IsNull(result.AbsoluteExpirationDateUTC);
          Aver.IsTrue(ds.Delete(tbl, key));
          r = ds.Get(tbl, key);
          Aver.IsNull(r);
        }
      }

      [Run]
      public void KDB_T00011_PutGetRowWithAttributes()
      {
        using(var app = new ServiceBaseApplication(null, CONFIG_1.AsLaconicConfig(handling: ConvertErrorHandling.Throw)))
        {
          var tbl = "tbla";
          DropCollection(tbl);

          var ds = app.DataStore as DefaultKDBStore;
          var key = new byte[]{1,2,3};
          var absExp = new DateTime(2300, 1, 1, 14, 00, 00, DateTimeKind.Utc);
          ds.Put(tbl, key, new TestRow {Name="SomeName", Description="SomeDescr"}, 1234, absExp);

          var result = ds.Get<TestRow>(tbl, key);
          Aver.IsTrue(result.IsAssigned);
          Aver.AreEqual("SomeName", result.Value.Name);
          Aver.AreEqual("SomeDescr", result.Value.Description);
          Aver.AreEqual(1234, result.SlidingExpirationDays);
          Aver.AreEqual(absExp, result.AbsoluteExpirationDateUTC);
        }
      }

      [Run]
      public void KDB_T00012_GetPutDeleteRaw()
      {
        using(var app = new ServiceBaseApplication(null, CONFIG_1.AsLaconicConfig(handling: ConvertErrorHandling.Throw)))
        {
          var tbl = "tbla";
          DropCollection(tbl);

          var ds = app.DataStore as DefaultKDBStore;
          var key = "key1".ToUTF8Bytes();
          var raw = ds.GetRaw(tbl, key);
          Aver.IsFalse(raw.IsAssigned);
          ds.PutRaw(tbl, key, new byte[]{1,2,3,4,5});
          var result = ds.GetRaw(tbl, key);
          Aver.IsTrue(result.IsAssigned);
          Aver.IsNotNull(result.Value);
          Aver.AreEqual(5, result.Value.Length);

          Aver.AreEqual(1, result.Value[0]);
          Aver.AreEqual(5, result.Value[4]);

          Aver.IsTrue(ds.Delete(tbl, key));
          raw = ds.GetRaw(tbl, key);
          Aver.IsFalse(raw.IsAssigned);
        }
      }



      [Run]
      public void KDB_T00020_GetPutMultipleTables()
      {
        using(var app = new ServiceBaseApplication(null, CONFIG_1.AsLaconicConfig(handling: ConvertErrorHandling.Throw)))
        {
          var tbl = "tbla";
          var tbl2 = "tblb";
          var tbl3 = "tblB";
          DropCollection(tbl);
          DropCollection(tbl2);
          DropCollection(tbl3);

          var ds = app.DataStore as DefaultKDBStore;
          var key = "key2".ToUTF8Bytes();

          ds.Put(tbl, key, new TestRow {Name="na", Description="da"});
          ds.Put(tbl2, key, new TestRow {Name="nb", Description="db"});
          ds.Put(tbl3, key, new TestRow {Name="nB", Description="dB"});

          var r = ds.Get<TestRow>(tbl, key);
          var r2 = ds.Get<TestRow>(tbl2, key);
          var r3 = ds.Get<TestRow>(tbl3, key);

          Aver.AreEqual("na", r.Value.Name);
          Aver.AreEqual("da", r.Value.Description);

          Aver.AreEqual("nb", r2.Value.Name);
          Aver.AreEqual("db", r2.Value.Description);

          Aver.AreEqual("nB", r3.Value.Name);
          Aver.AreEqual("dB", r3.Value.Description);
        }
      }

      [Run("cnt=25000  parallel=false")]
      [Run("cnt=25000  parallel=true")]
      [Run("cnt=100111 parallel=false")]
      [Run("cnt=100111 parallel=true")]
      public void KDB_T01000_Speed_PutRow(int cnt, bool parallel)
      {
        var tbl = "tbla";
        using(var app = new ServiceBaseApplication(null, CONFIG_1.AsLaconicConfig(handling: ConvertErrorHandling.Throw)))
        {
          var ds = app.DataStore as DefaultKDBStore;
          DropCollection(tbl);

          var row = new TwoGdidRow();
          var sw = Stopwatch.StartNew();
          if (parallel)
          {
            Parallel.For(0,cnt,(i)=>{
              row.G1 = new GDID(0,1,(ulong)i);
              row.G2 = new GDID(125,4,(ulong)i);
              var key = "key{0}".Args(i).ToUTF8Bytes();
              ds.Put(tbl, key, row);
            });
          }
          else
          {
            for(var i=0; i<cnt; i++)
            {
              row.G1 = new GDID(0,1,(ulong)i);
              row.G2 = new GDID(125,4,(ulong)i);
              var key = "key{0}".Args(i).ToUTF8Bytes();
              ds.Put(tbl, key, row);
            }
          }
          sw.Stop();
          Console.WriteLine("Did {0:n2} in {1:n2} msec. at {2:n2} ops/sec".Args(cnt, sw.ElapsedMilliseconds, cnt/(sw.ElapsedMilliseconds/(double)1000)));
        }
      }

      [Run("cnt=25000  parallel=false")]
      [Run("cnt=25000  parallel=true")]
      [Run("cnt=100111 parallel=false")]
      [Run("cnt=100111 parallel=true")]
      public void KDB_T01010_Speed_GetRow(int cnt, bool parallel)
      {
        var tbl = "tbla";
        using(var app = new ServiceBaseApplication(null, CONFIG_1.AsLaconicConfig(handling: ConvertErrorHandling.Throw)))
        {
          var ds = app.DataStore as DefaultKDBStore;
          DropCollection(tbl);

          var row = new TwoGdidRow();

          Parallel.For(0,cnt,(i)=>{
              row.G1 = new GDID(0,1,(ulong)i);
              row.G2 = new GDID(125,4,(ulong)i);
              var key = i.ToString().ToUTF8Bytes();
              ds.Put(tbl, key, row);
            });

          var sw = Stopwatch.StartNew();
          if (parallel)
          {
            Parallel.For(0,cnt,(i)=>{
               Aver.IsTrue(ds.Get<TwoGdidRow>(tbl, i.ToString().ToUTF8Bytes()).IsAssigned);
            });
          }
          else
          {
            for(var i=0; i<cnt; i++)
            {
              Aver.IsTrue(ds.Get<TwoGdidRow>(tbl, i.ToString().ToUTF8Bytes()).IsAssigned);
            }
          }
          sw.Stop();
          Console.WriteLine("Did {0:n2} in {1:n2} msec. at {2:n2} ops/sec".Args(cnt, sw.ElapsedMilliseconds, cnt/(sw.ElapsedMilliseconds/(double)1000)));
        }
      }


      [Run("cnt=25000  parallel=false")]
      [Run("cnt=25000  parallel=true")]
      [Run("cnt=100111 parallel=false")]
      [Run("cnt=100111 parallel=true")]
      public void KDB_T01020_Speed_GetRaw(int cnt, bool parallel)
      {
        var tbl = "tblRaw";
        using(var app = new ServiceBaseApplication(null, CONFIG_1.AsLaconicConfig(handling: ConvertErrorHandling.Throw)))
        {
          var ds = app.DataStore as DefaultKDBStore;
          DropCollection(tbl);

          Parallel.For(0,cnt,(i)=>{
              var data = new byte[2*12];
              var key = i.ToString().ToUTF8Bytes();
              ds.PutRaw(tbl, key, data);
            });

          var sw = Stopwatch.StartNew();
          if (parallel)
          {
            Parallel.For(0,cnt,(i)=>{
               Aver.IsTrue(ds.GetRaw(tbl, i.ToString().ToUTF8Bytes()).IsAssigned);
            });
          }
          else
          {
            for(var i=0; i<cnt; i++)
            {
              Aver.IsTrue(ds.GetRaw(tbl, i.ToString().ToUTF8Bytes()).IsAssigned);
            }
          }
          sw.Stop();
          Console.WriteLine("Did {0:n2} in {1:n2} msec. at {2:n2} ops/sec".Args(cnt, sw.ElapsedMilliseconds, cnt/(sw.ElapsedMilliseconds/(double)1000)));
        }
      }

      [Run]
      public void KDB_T020000_Fallback()
      {
        const string TBL = "tfb";
        // step0: Delete all data
        DropCollection(TBL);
        // step1: Before fallback
        using(var app = new ServiceBaseApplication(null, CONFIG_FALLBACK_1.AsLaconicConfig(handling: ConvertErrorHandling.Throw)))
        {
          var ds = app.DataStore as DefaultKDBStore;
          ds.PutRaw(TBL, "1".ToUTF8Bytes() , "data1".ToUTF8Bytes());
          ds.PutRaw(TBL, "2".ToUTF8Bytes() , "data2".ToUTF8Bytes());
          ds.PutRaw(TBL, "3".ToUTF8Bytes() , "data3".ToUTF8Bytes());
          ds.PutRaw(TBL, "4".ToUTF8Bytes() , "data4".ToUTF8Bytes());

          Aver.AreEqual("data1", ds.GetRaw(TBL, "1".ToUTF8Bytes()).Value.FromUTF8Bytes());
          Aver.AreEqual("data2", ds.GetRaw(TBL, "2".ToUTF8Bytes()).Value.FromUTF8Bytes());
          Aver.AreEqual("data3", ds.GetRaw(TBL, "3".ToUTF8Bytes()).Value.FromUTF8Bytes());
          Aver.AreEqual("data4", ds.GetRaw(TBL, "4".ToUTF8Bytes()).Value.FromUTF8Bytes());

          ds.PutRaw(TBL, "up1".ToUTF8Bytes(), "updata1".ToUTF8Bytes());
          ds.PutRaw(TBL, "up2".ToUTF8Bytes(), "updata2".ToUTF8Bytes());
          ds.PutRaw(TBL, "up3".ToUTF8Bytes(), "updata3".ToUTF8Bytes());
          ds.PutRaw(TBL, "up4".ToUTF8Bytes(), "updata4".ToUTF8Bytes());
        }

        // step2: Fallback
        using(var app = new ServiceBaseApplication(null, CONFIG_FALLBACK_2.AsLaconicConfig(handling: ConvertErrorHandling.Throw)))
        {
          var ds = app.DataStore as DefaultKDBStore;
          Aver.IsNotNull(ds.RootShardSet.Fallback);
          Aver.AreObjectsEqual(ds.RootShardSet, ds.RootShardSet.Fallback.FallbackParent);
          Aver.AreEqual(1, ds.RootShardSet.Fallback.FallbackLevel);

          ds.PutRaw(TBL, "1".ToUTF8Bytes() , "data1".ToUTF8Bytes());
          ds.PutRaw(TBL, "2".ToUTF8Bytes() , "data2".ToUTF8Bytes());
          ds.PutRaw(TBL, "3".ToUTF8Bytes() , "data3".ToUTF8Bytes());
          ds.PutRaw(TBL, "4".ToUTF8Bytes() , "data4".ToUTF8Bytes());
          ds.PutRaw(TBL, "5".ToUTF8Bytes() , "data5".ToUTF8Bytes());
          ds.PutRaw(TBL, "6".ToUTF8Bytes() , "data6".ToUTF8Bytes());
          ds.PutRaw(TBL, "7".ToUTF8Bytes() , "data7".ToUTF8Bytes());
          ds.PutRaw(TBL, "8".ToUTF8Bytes() , "data8".ToUTF8Bytes());

          Aver.AreEqual("data1", ds.GetRaw(TBL, "1".ToUTF8Bytes()).Value.FromUTF8Bytes());
          Aver.AreEqual("data2", ds.GetRaw(TBL, "2".ToUTF8Bytes()).Value.FromUTF8Bytes());
          Aver.AreEqual("data3", ds.GetRaw(TBL, "3".ToUTF8Bytes()).Value.FromUTF8Bytes());
          Aver.AreEqual("data4", ds.GetRaw(TBL, "4".ToUTF8Bytes()).Value.FromUTF8Bytes());
          Aver.AreEqual("data5", ds.GetRaw(TBL, "5".ToUTF8Bytes()).Value.FromUTF8Bytes());
          Aver.AreEqual("data6", ds.GetRaw(TBL, "6".ToUTF8Bytes()).Value.FromUTF8Bytes());
          Aver.AreEqual("data7", ds.GetRaw(TBL, "7".ToUTF8Bytes()).Value.FromUTF8Bytes());
          Aver.AreEqual("data8", ds.GetRaw(TBL, "8".ToUTF8Bytes()).Value.FromUTF8Bytes());

          ds.Delete(TBL, "1".ToUTF8Bytes());
          ds.Delete(TBL, "2".ToUTF8Bytes());
          ds.Delete(TBL, "7".ToUTF8Bytes());
          ds.Delete(TBL, "8".ToUTF8Bytes());

          Aver.IsFalse(ds.GetRaw(TBL, "1".ToUTF8Bytes()).IsAssigned);
          Aver.IsFalse(ds.GetRaw(TBL, "2".ToUTF8Bytes()).IsAssigned);
          Aver.IsFalse(ds.GetRaw(TBL, "7".ToUTF8Bytes()).IsAssigned);
          Aver.IsFalse(ds.GetRaw(TBL, "8".ToUTF8Bytes()).IsAssigned);
          Aver.IsTrue(ds.GetRaw(TBL, "3".ToUTF8Bytes()).IsAssigned);
          Aver.IsTrue(ds.GetRaw(TBL, "4".ToUTF8Bytes()).IsAssigned);
          Aver.IsTrue(ds.GetRaw(TBL, "5".ToUTF8Bytes()).IsAssigned);
          Aver.IsTrue(ds.GetRaw(TBL, "6".ToUTF8Bytes()).IsAssigned);

          Aver.AreEqual("updata1", ds.GetRaw(TBL, "up1".ToUTF8Bytes()).Value.FromUTF8Bytes());
          Aver.AreEqual("updata2", ds.GetRaw(TBL, "up2".ToUTF8Bytes()).Value.FromUTF8Bytes());
          Aver.AreEqual("updata3", ds.GetRaw(TBL, "up3".ToUTF8Bytes()).Value.FromUTF8Bytes());
          Aver.AreEqual("updata4", ds.GetRaw(TBL, "up4".ToUTF8Bytes()).Value.FromUTF8Bytes());
        }
      }
  }
}
