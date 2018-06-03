using System;
using System.Collections.Generic;
using System.Linq;
using NFX.Scripting;

using Agni.Contracts;

using NFX;
using NFX.ApplicationModel;

namespace Agni.UTest
{
  [Runnable]
  public class ClientHubTests : BaseTestRigWithMetabase
  {

     const string CONFIG1 =
@"
nfx
{
  glue
  {
    bindings
    {
      binding
      {
        name='sync'
        type='NFX.Glue.Native.SyncBinding, NFX'
      }
    }

    servers
    {
      server
      {
        name='TestServerSync'
        node='$(~SysConsts.SYNC_BINDING)://*:$(~SysConsts.NETWORK_SVC_TESTER_SYNC_PORT)'
        contract-servers='Agni.UTest.ClientHubTests+TestingServer, Agni.UTest'
      }
    }

  }
}
";

     internal class TestingServer : Agni.Contracts.ITester
     {
       public object TestEcho(object data)
       {
         if (data is string && (string)data=="FAIL") throw new NFXException("I failed!");
         return data;
       }
     }



      [Run]
      public void CH_EchoOneCall()
      {
         var conf  = CONFIG1.AsLaconicConfig(handling: ConvertErrorHandling.Throw );
         using(var app =  new ServiceBaseApplication(null, conf))
         {
              using( var tester = ServiceClientHub.New<ITesterClient>("/us/east/cle/a/ii/wmed0004"))
              {
                 var arg = "Abcdefg";
                 var echoed = tester.TestEcho(arg);
                 Aver.AreObjectsEqual( arg, echoed );
              }
        }
      }

      [Run]
      public void CH_EchoTwoCalls()
      {
         var conf  = CONFIG1.AsLaconicConfig(handling: ConvertErrorHandling.Throw );
         using(var app =  new ServiceBaseApplication(null, conf))
         {
              using( var tester = ServiceClientHub.New<ITesterClient>("/us/east/cle/a/ii/wmed0004"))
              {
                 var arg = "Abcdefg";
                 var echoed = tester.TestEcho(arg);
                 Aver.AreObjectsEqual( arg, echoed );

                 arg = "Ze Rozenberg";
                 echoed = tester.TestEcho(arg);
                 Aver.AreObjectsEqual( arg, echoed );
              }
        }
      }


      [Run]
      public void CH_CallWithRetry_FirstHostErr()
      {
         var conf  = CONFIG1.AsLaconicConfig(handling: ConvertErrorHandling.Throw );
         using(var app =  new ServiceBaseApplication(null, conf))
         {
              Exception err = null;
              var arg = "Abcdefg";
              var echoed = ServiceClientHub.CallWithRetry<ITesterClient, object>
              (
                (cl) => cl.TestEcho( arg ),
                new string[]{ "/us/east/cle/a/i/wmed0002", "/us/east/cle/a/ii/wmed0004"},
                (cl, er) => { err = er; Console.WriteLine(er.ToMessageWithType()); return false; }
              );

              Aver.AreObjectsEqual( arg, echoed );
              Aver.IsTrue( err.ToMessageWithType().Contains("address is not valid"));
        }
      }

      [Run]
      [Aver.Throws(Message="address is not valid", MsgMatch=Aver.ThrowsAttribute.MatchType.Contains)]
      public void CH_CallWithRetry_HostErrAbort()
      {
         var conf  = CONFIG1.AsLaconicConfig(handling: ConvertErrorHandling.Throw );
         using(var app =  new ServiceBaseApplication(null, conf))
         {
              Exception err = null;
              var arg = "Abcdefg";
              var echoed = ServiceClientHub.CallWithRetry<ITesterClient, object>
              (
                (cl) => cl.TestEcho( arg ),
                new string[]{ "/us/east/cle/a/i/wmed0002", "/us/east/cle/a/i/wmed0001"},
                (cl, er) => { err = er; Console.WriteLine(er.ToMessageWithType()); return true; }
              );
        }
      }


      [Run]
      public void CH_CallWithRetryAsync_FirstTwoHostErr()
      {
         var conf  = CONFIG1.AsLaconicConfig(handling: ConvertErrorHandling.Throw );
         using(var app =  new ServiceBaseApplication(null, conf))
         {
              var err = new List<Exception>();
              var arg = "Abcdefg";
              var echoed = ServiceClientHub.CallWithRetryAsync<ITesterClient, object>
              (
                (cl) => cl.Async_TestEcho( arg ).AsTaskReturning<object>(),
                new string[]{ "/us/east/cle/a/i/wmed0002", "/us/east/cle/a/i/wmed0002", "/us/east/cle/a/ii/wmed0004"},
                (cl, er) => { err.Add(er); Console.WriteLine(er.ToMessageWithType()); return false; }
              );

              Aver.AreObjectsEqual( arg, echoed.Result );
              Aver.IsTrue( err[0].ToMessageWithType().Contains("address is not valid"));
              Aver.IsTrue( err[1].ToMessageWithType().Contains("address is not valid"));
        }
      }

      [Run]
      [Aver.Throws(Message="address is not valid")]
      public void CH_CallWithRetryAsync_FirstHostErrAbort()
      {
         var conf  = CONFIG1.AsLaconicConfig(handling: ConvertErrorHandling.Throw );
         using(var app =  new ServiceBaseApplication(null, conf))
         {
              Exception err = null;
              var arg = "Abcdefg";
              var echoed = ServiceClientHub.CallWithRetryAsync<ITesterClient, object>
              (
                (cl) => cl.Async_TestEcho( arg ).AsTaskReturning<object>(),
                new string[]{ "/us/east/cle/a/i/wmed0002", "/us/east/cle/a/i/wmed0001"},
                (cl, er) => { err = er; Console.WriteLine(er.ToMessageWithType()); return true; }
              );

              try
              {
                var r = echoed.Result;
              }
              catch (AggregateException ae)
              {
                throw ae.GetBaseException();
              }
        }
      }


      [Run]
      [Aver.Throws(typeof(NFX.Glue.RemoteException), Message="I failed")]
      public void CH_EchoFAIL()
      {
         var conf  = CONFIG1.AsLaconicConfig(handling: ConvertErrorHandling.Throw );
         using(var app =  new ServiceBaseApplication(null, conf))
         {
              using( var tester = ServiceClientHub.New<ITesterClient>("/us/east/cle/a/ii/wmed0004"))
              {
                 var arg = "FAIL";
                 var echoed = tester.TestEcho(arg);
              }
        }
      }


      [Run]
      [Aver.Throws(typeof(NFX.Glue.RemoteException), Message="I failed")]
      public void CH_CallWithRetry_TwoHostsErr()
      {
         var conf  = CONFIG1.AsLaconicConfig(handling: ConvertErrorHandling.Throw );
         using(var app =  new ServiceBaseApplication(null, conf))
         {
              Exception err = null;
              var arg = "FAIL";
              var echoed = ServiceClientHub.CallWithRetry<ITesterClient, object>
              (
                (cl) => cl.TestEcho( arg ),
                new string[]{ "/us/east/cle/a/i/wmed0001", "/us/east/cle/a/ii/wmed0004"},
                (cl, er) => { err = er; Console.WriteLine(er.ToMessageWithType()); return er is NFX.Glue.RemoteException; }
              );
        }
      }


      [Run]
      [Aver.Throws(typeof(Agni.Clients.AgniClientException), Message="after 2 retries")]
      public void CH_CallWithRetry_TwoHostErrAbortFalse()
      {
         var conf  = CONFIG1.AsLaconicConfig(handling: ConvertErrorHandling.Throw );
         using(var app =  new ServiceBaseApplication(null, conf))
         {
              Exception err = null;
              var arg = "FAIL";
              var echoed = ServiceClientHub.CallWithRetry<ITesterClient, object>
              (
                (cl) => cl.TestEcho( arg ),
                new string[]{ "/us/east/cle/a/i/wmed0001", "/us/east/cle/a/i/wmed0001"},
                (cl, er) => { err = er; Console.WriteLine(er.ToMessageWithType()); return false; }
              );
        }
      }


      [Run]
      [Aver.Throws(typeof(NFX.Glue.RemoteException), Message="I failed")]
      public void CH_CallWithRetryAsync_TwoHostsErr()
      {
         var conf  = CONFIG1.AsLaconicConfig(handling: ConvertErrorHandling.Throw );
         using(var app =  new ServiceBaseApplication(null, conf))
         {
              Exception err = null;
              var arg = "FAIL";
              var echoed = ServiceClientHub.CallWithRetryAsync<ITesterClient, object>
              (
                (cl) => cl.Async_TestEcho( arg ).AsTaskReturning<object>(),
                new string[]{ "/us/east/cle/a/i/wmed0001", "/us/east/cle/a/ii/wmed0004"},
                (cl, er) => {
                  err = er; Console.WriteLine(er.ToMessageWithType()); return er is NFX.Glue.RemoteException;
                }
              );

              try
              { var x = echoed.Result; }
              catch(AggregateException ae)
              {
                throw ae.GetBaseException();
              }
        }
      }

      [Run]
      public void CH_ClientCallRetryAsync_TaskVoid()
      {
        var conf = CONFIG1.AsLaconicConfig(handling: ConvertErrorHandling.Throw);
        using (new ServiceBaseApplication(null, conf))
        {
          var echoed = ServiceClientHub.CallWithRetryAsync<ITesterClient>(
            (cl) => cl.Async_TestEcho( "test" ).AsTaskReturning<object>(),
            new string[]{ "/us/east/cle/a/i/wmed0002", "/us/east/cle/a/i/wmed0001"},
            (cl, er) => {
              Console.WriteLine(er.ToMessageWithType()); return false; }
          );
        }
      }

  }
}
