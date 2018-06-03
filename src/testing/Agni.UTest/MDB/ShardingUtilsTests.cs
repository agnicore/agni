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
  public class ShardingUtilsTests : BaseTestRigWithMetabase
  {
    [Run]
    public void RandomWebSafeStringToShardingIDTest()
    {
      var count = 1000000L;
      var counters = new long[8];
      for (var i = 0L; i < count; i++)
      {
        var shardID = ShardingUtils.StringToShardingID(ExternalRandomGenerator.Instance.NextRandomWebSafeString(4, 12));
        counters[shardID % (ulong)counters.Length]++;
      }
      for (int i = 0, length = counters.Length; i < length; i++)
      {
        var percent = (counters[i] * 10000 / count) / 100.0;
        Console.WriteLine("counter[{0}] = {1}; // {2}%".Args(i, counters[i], percent));
        Aver.IsTrue(Math.Abs((100.0 / counters.Length) - percent) <= 1.0);
      }
    }
  }
}