﻿using System;
using System.Threading;

using NFX;
using NFX.IO;

using Agni.AppModel;
using Agni.AppModel.HostGovernor;

namespace Agni.Hosts.ahgov
{
  public static class ProgramBody
  {
    public static void Main(string[] args)
    {
      try
      {
        AgniSystem.MetabaseApplicationName = SysConsts.APP_NAME_HGOV;

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
      const string FROM = "AHGOV.Program";

      using (var app = new AgniServiceApplication(SystemApplicationType.HostGovernor, args, null))
      {
        try
        {
          var fromARD = app.CommandArgs[SysConsts.ARD_PARENT_CMD_PARAM].Exists;
          var updateProblem = app.CommandArgs[SysConsts.ARD_UPDATE_PROBLEM_CMD_PARAM].Exists;
          using (var governor = new HostGovernorService(fromARD, updateProblem))
          {
            governor.Configure(null);
            governor.Start();
            try
            {
              // WARNING: Do not modify what this program reads/writes from/to standard IO streams because
              // ARD uses those particular string messages for its protocol
              Console.WriteLine("OK."); //<-- ARD protocol, ARD waits for this token to assess startup situation
              ConsoleUtils.WriteMarkupContent(typeof(ProgramBody).GetText("Welcome.txt"));
              Console.WriteLine("Waiting for line to terminate...");


              var abortableConsole = new TerminalUtils.AbortableLineReader();
              try
              {
                while (app.Active && !governor.NeedsProcessRestart)
                {
                  if (abortableConsole.Line != null)
                  {
                    app.Log.Write(new NFX.Log.Message
                    {
                      Type = NFX.Log.MessageType.Info,
                      Topic = SysConsts.LOG_TOPIC_APP_MANAGEMENT,
                      From = FROM,
                      Text = "Main loop received CR|LF. Exiting..."
                    });
                    break;  //<-- ARD protocol, ARD sends a <CRLF> when it is time to shut down
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
              governor.WaitForCompleteStop();
            }
          }//using governor
        }
        catch (Exception error)
        {
          app.Log.Write(new NFX.Log.Message
          {
            Type = NFX.Log.MessageType.CatastrophicError,
            Topic = SysConsts.LOG_TOPIC_APP_MANAGEMENT,
            From = FROM,
            Text = "Exception leaked in run(): " + error.ToMessageWithType(),
            Exception = error
          });

          throw error;
        }
      }//using APP
    }
  }
}
