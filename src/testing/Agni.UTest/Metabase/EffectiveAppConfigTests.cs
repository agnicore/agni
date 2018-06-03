﻿using System.Threading;
using System.Threading.Tasks;
using NFX.Scripting;

using NFX;
using NFX.IO.FileSystem.Local;

using Agni.Metabase;
using Agni.AppModel;

namespace Agni.UTest.Metabase
{
  [Runnable]
  public class EffectiveAppConfigTests
  {
      const string WMED0001 = "us/east/cle/a/i/wmed0001.h";
      const string WMED0002 = "us/east/cle/a/i/wmed0002.h";
      const string WMED0003 = "us/east/cle/a/ii/wmed0003.h";
      const string WMED0004 = "us/east/cle/a/ii/wmed0004.h";

      [Run]
      public void EC_WebApp_WMED0001()
      {
        using(var fs = new LocalFileSystem(null))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        using (var session = BootConfLoader.LoadForTest(SystemApplicationType.TestRig, mb, TestSources.THIS_HOST))
        {
          var host = mb.CatalogReg.NavigateHost(WMED0001);

          var conf = host.GetEffectiveAppConfig("WebApp");

          Aver.AreEqual("WebApp", conf.Navigate("/$application-name").Value);
          Aver.AreEqual("wmed0001", conf.Navigate("/$name").Value);

          Aver.AreEqual("A", conf.Navigate("/gv/$a").Value);
          Aver.AreEqual("B", conf.Navigate("/gv/$b").Value);

          Aver.IsNull(conf.Navigate("/glue/$zoloto").Value);
          Aver.AreEqual("da", conf.Navigate("/glue/$serebro").Value);

          Aver.AreEqual(123, conf.Navigate("/east-block/$something").ValueAsInt());
          Aver.IsTrue(conf.Navigate("/cleveland/$solon").ValueAsBool());
          Aver.IsFalse(conf.Navigate("/cleveland/$hudson").ValueAsBool());
          Aver.AreEqual("1.2.0890b", conf.Navigate("/windows7/$build").Value);
        }
      }

      [Run]
      public void EC_WebApp1_WMED0004()
      {
        using(var fs = new LocalFileSystem(null))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        using (var session = BootConfLoader.LoadForTest(SystemApplicationType.TestRig, mb, TestSources.THIS_HOST))
        {
          var host = mb.CatalogReg.NavigateHost(WMED0004);

          var conf = host.GetEffectiveAppConfig("WebApp1");

          Aver.AreEqual("WebApp1", conf.Navigate("/$application-name").Value);
          Aver.AreEqual("wmed0004", conf.Navigate("/$name").Value);
          Aver.AreEqual("Popov", conf.Navigate("/$clown").Value);

          Aver.AreEqual("A", conf.Navigate("/gv/$a").Value);
          Aver.AreEqual("B", conf.Navigate("/gv/$b").Value);

          Aver.AreEqual("da", conf.Navigate("/glue/$zoloto").Value);
          Aver.AreEqual("da", conf.Navigate("/glue/$serebro").Value);

          Aver.AreEqual(123, conf.Navigate("/east-block/$something").ValueAsInt());
          Aver.AreEqual(true, conf.Navigate("/cleveland/$solon").ValueAsBool());
          Aver.AreEqual(true, conf.Navigate("/cleveland/$hudson").ValueAsBool());
          Aver.IsFalse(conf.Navigate("/windows7/$build").Exists);
        }
      }

      [Run]
      public void EC_WinFormsTest_WMED0004()
      {
        using(var fs = new LocalFileSystem(null))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        using (var session = BootConfLoader.LoadForTest(SystemApplicationType.TestRig, mb, TestSources.THIS_HOST))
        {
          var host = mb.CatalogReg.NavigateHost(WMED0004);

          var conf = host.GetEffectiveAppConfig("WinFormsTest");

          Aver.AreEqual("WinFormsTest on HOST1", conf.Navigate("/$application-name").Value);
          Aver.AreEqual("Nikulin", conf.Navigate("/$clown").Value);

          Aver.AreEqual("A", conf.Navigate("/gv/$a").Value);
          Aver.AreEqual("B", conf.Navigate("/gv/$b").Value);

          Aver.IsNull(conf.Navigate("/glue/$zoloto").Value);
          Aver.AreEqual("da", conf.Navigate("/glue/$serebro").Value);

          Aver.AreEqual(123, conf.Navigate("/east-block/$something").ValueAsInt());
          Aver.IsTrue(conf.Navigate("/cleveland/$solon").ValueAsBool());
          Aver.IsFalse(conf.Navigate("/cleveland/$hudson").ValueAsBool());
          Aver.IsFalse(conf.Navigate("/windows7/$build").Exists);
        }
      }


      [Run]
      [Aver.Throws(typeof(MetabaseException), Message="is not a part of agni role", MsgMatch= Aver.ThrowsAttribute.MatchType.Contains)]
      public void EC_Server_fail()
      {
        using(var fs = new LocalFileSystem(null))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        using (var session = BootConfLoader.LoadForTest(SystemApplicationType.TestRig, mb, TestSources.THIS_HOST))
        {
          var host = mb.CatalogReg.NavigateHost(WMED0002);

          var conf = host.GetEffectiveAppConfig("TestApp");
        }
      }

      [Run]
      public void EC_Server_success()
      {
        using(var fs = new LocalFileSystem(null))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        using (var session = BootConfLoader.LoadForTest(SystemApplicationType.TestRig, mb, TestSources.THIS_HOST))
        {
          var host = mb.CatalogReg.NavigateHost(WMED0003);

          var conf = host.GetEffectiveAppConfig("AZGov");

          Aver.IsFalse(conf.Navigate("/windows7/$build").Exists);
        }
      }


      [Run]
      public void EC_Various_Parallel()
      {
        using(var fs = new LocalFileSystem(null))
        using(var mb = new Metabank(fs, null, TestSources.RPATH))
        using (var session = BootConfLoader.LoadForTest(SystemApplicationType.TestRig, mb, TestSources.THIS_HOST))
        {
          Parallel.For(0, TestSources.PARALLEL_LOOP_TO, (i) =>
          {
            Thread.SpinWait(ExternalRandomGenerator.Instance.NextScaledRandomInteger(10, 2000));

            var host = mb.CatalogReg.NavigateHost(WMED0004);

            var conf = host.GetEffectiveAppConfig("WebApp1");

            Aver.AreEqual("WebApp1", conf.Navigate("/$application-name").Value);

            Aver.AreEqual("A", conf.Navigate("/gv/$a").Value);
            Aver.AreEqual("B", conf.Navigate("/gv/$b").Value);

            Aver.AreEqual("da", conf.Navigate("/glue/$zoloto").Value);
            Aver.AreEqual("da", conf.Navigate("/glue/$serebro").Value);

            Aver.AreEqual(123, conf.Navigate("/east-block/$something").ValueAsInt());
            Aver.AreEqual(true, conf.Navigate("/cleveland/$solon").ValueAsBool());
            Aver.AreEqual(true, conf.Navigate("/cleveland/$hudson").ValueAsBool());
            Aver.IsFalse(conf.Navigate("/windows7/$build").Exists);


            //--------------------------------------------
            Thread.SpinWait(ExternalRandomGenerator.Instance.NextScaledRandomInteger(10, 2000));
            host = mb.CatalogReg.NavigateHost(WMED0002);
            conf = host.GetEffectiveAppConfig("WebApp");
            Aver.AreEqual("1.2.0890b", conf.Navigate("/windows7/$build").Value);

            //--------------------------------------------
            Thread.SpinWait(ExternalRandomGenerator.Instance.NextScaledRandomInteger(10, 2000));
            host = mb.CatalogReg.NavigateHost(WMED0004);

            conf = host.GetEffectiveAppConfig("WinFormsTest");

            Aver.AreEqual("WinFormsTest on HOST1", conf.Navigate("/$application-name").Value);

            Aver.AreEqual("A", conf.Navigate("/gv/$a").Value);
            Aver.AreEqual("B", conf.Navigate("/gv/$b").Value);

            Aver.IsNull(conf.Navigate("/glue/$zoloto").Value);
            Aver.AreEqual("da", conf.Navigate("/glue/$serebro").Value);

            Aver.AreEqual(123, conf.Navigate("/east-block/$something").ValueAsInt());
            Aver.AreEqual(true, conf.Navigate("/cleveland/$solon").ValueAsBool());
            Aver.AreEqual(false, conf.Navigate("/cleveland/$hudson").ValueAsBool());

            Aver.IsFalse(conf.Navigate("/windows7/$build").Exists);
          });
        }
      }
  }
}
