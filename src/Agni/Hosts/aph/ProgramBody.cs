using System;
using System.Threading;

using NFX;
using NFX.IO;

using Agni.AppModel;
using Agni.Workers.Server;

namespace Agni.Hosts.aph
{
  public static class ProgramBody
  {
    public static void Main(string[] args)
    {
      try
      {
        //Specify the name in boot conf under:
        //agni
        //{
        //  metabase
        //  {
        //    app-name="XXXXXXXX"
        //  }
        //}

        //OR
          //Inject from command line:  aph -agni app-name=<your app name>


        //DO NOT USE static process assignment
          //AgniSystem.MetabaseApplicationName = Agni.SysConsts.APP_NAME_APH;

        run(args);

        Environment.ExitCode = 0;
      }
      catch (Exception error)
      {
        Console.WriteLine(error.ToString());
        Environment.ExitCode = -1;
      }
    }

    static void run(string[] args)
    {
      const string FROM = "APH.Program";

      using (var app = new AgniServiceApplication(SystemApplicationType.ProcessHost, args, null))
      {
        try
        {
          using (var procHost = new ProcessControllerService(null))
          {
            procHost.Configure(null);
            procHost.Start();
            try
            {
              // WARNING: Do not modify what this program reads/writes from/to standard IO streams because
              //  AHGOV uses those particular string messages for its protocol
              Console.WriteLine("OK."); //<-- AHGOV protocol, AHGOV waits for this token to assess startup situation
              ConsoleUtils.WriteMarkupContent(typeof(ProgramBody).GetText("Welcome.txt"));
              Console.WriteLine("Waiting for line to terminate...");

              var abortableConsole = new TerminalUtils.AbortableLineReader();
              try
              {
                while (app.Active)
                {
                  if (abortableConsole.Line != null)
                  {
                    app.Log.Write(new NFX.Log.Message
                    {
                      Type = NFX.Log.MessageType.Info,
                      Topic = Agni.SysConsts.LOG_TOPIC_WWW,
                      From = FROM,
                      Text = "Main loop received CR|LF. Exiting..."
                    });
                    break;  //<-- AHGOV protocol, AHGOV sends a <CRLF> when it is time to shut down
                  }
                  Thread.Sleep(1000);
                }
              }
              finally
              {
                abortableConsole.Abort();
              }
            }
            finally
            {
              procHost.WaitForCompleteStop();
            }
          }
        }
        catch (Exception error)
        {
          app.Log.Write(new NFX.Log.Message
          {
            Type = NFX.Log.MessageType.CatastrophicError,
            Topic = Agni.SysConsts.LOG_TOPIC_SVC,
            From = FROM,
            Text = "Exception leaked in run(): " + error.ToMessageWithType(),
            Exception = error
          });

          throw error;
        }
      }//using app
    }
  }
}
