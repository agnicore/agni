using System;

using NFX;
using NFX.IO.FileSystem.Local;
using NFX.Scripting;

using Agni.AppModel;
using Agni.Metabase;

namespace Agni.UTest.Metabase
{
  [Runnable]
  public class MetabankIncludeTest
  {
    const string WMED0004 = "us/east/cle/a/ii/wmed0004.h";

    [Run]
    public void MI_Test()
    {
      using (var fs = new LocalFileSystem(null))
      using(var mb = new Metabank(fs, null, TestSources.RPATH))
      using (var session = BootConfLoader.LoadForTest(SystemApplicationType.TestRig, mb, TestSources.THIS_HOST))
      {
        var host = mb.CatalogReg.NavigateHost(WMED0004);

        var conf = host.GetEffectiveAppConfig("WebApp1");

        Aver.AreEqual("value", conf.Navigate("/$var").Value);
      }
    }
  }
}
