using System;
using System.Collections.Generic;
using System.Linq;

using NFX;
using NFX.IO.FileSystem.Local;
using NFX.Scripting;

using Agni.Metabase;

namespace Agni.UTest.Metabase
{
  [Runnable]
  public class ValidationTests
  {
      [Run]
      public void ValidateMetabank()
      {
        using(var fs = new LocalFileSystem(null))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        using(Agni.AppModel.BootConfLoader.LoadForTest(AppModel.SystemApplicationType.TestRig, mb, "US/East/CLE/A/I/wmed0001"))
        {
          var output = new List<MetabaseValidationMsg>();

          mb.Validate(output);

          foreach(var msg in output)
            Console.WriteLine(msg);

          Aver.AreEqual(5, output.Count(m=>m.Type== MetabaseValidationMessageType.Warning));
          Aver.AreEqual(6, output.Count(m=>m.Type== MetabaseValidationMessageType.Info));
        }
      }
  }
}
