using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX;
using NFX.Wave;
using NFX.Wave.MVC;
using NFX.DataAccess.Distributed;

using Agni.AppModel.Terminal;
using Agni.Security.Permissions.Admin;

namespace Agni.WebManager.Controllers
{
  /// <summary>
  /// Provides AppRemoteTerminal JSON API
  /// </summary>
  public sealed class RemoteTerminal : AWMController
  {
      public const string TERMINAL_SESSION_KEY = "app remote terminal instance";


      [Action]
      [RemoteTerminalOperatorPermission]
      public object Connect(string who = null)
      {
        WorkContext.NeedsSession();
        var terminal = WorkContext.Session[TERMINAL_SESSION_KEY] as AppRemoteTerminal;
        if (terminal!=null)
         return new {Status = "Already connected", WhenConnected = terminal.WhenConnected};


        if (who.IsNullOrWhiteSpace()) who = "{0}-{1}".Args(WorkContext.Request.UserHostAddress, WorkContext.Session.User);
        terminal = AppRemoteTerminal.MakeNewTerminal();
        var info = terminal.Connect(who);
        WorkContext.Session[TERMINAL_SESSION_KEY] = terminal;
        return info;
      }

      [Action]
      public object Disconnect()
      {
        WorkContext.NeedsSession();
        var terminal = WorkContext.Session[TERMINAL_SESSION_KEY] as AppRemoteTerminal;
        if (terminal==null)
         return new {Status = "Already disconnected"};

        var msg = terminal.Disconnect();
        terminal.Dispose();
        WorkContext.Session[TERMINAL_SESSION_KEY] = null;

        return new {Status = msg};
      }

      [Action]
      [RemoteTerminalOperatorPermission]
      [AppRemoteTerminalPermission]
      public object Execute(string command)
      {
        WorkContext.NeedsSession();
        var terminal = WorkContext.Session[TERMINAL_SESSION_KEY] as AppRemoteTerminal;
        if (terminal==null)
         return new {Status = "Error", Msg = "Not connected"};

        try
        {
          var result = terminal.Execute(command);

          var plainText = true;
          if (result.IsNotNullOrWhiteSpace())
            if (result.StartsWith(Agni.AppModel.Terminal.AppRemoteTerminal.MARKUP_PRAGMA))
            {
              result = result.Remove(0, Agni.AppModel.Terminal.AppRemoteTerminal.MARKUP_PRAGMA.Length);
              result = NFX.IO.ConsoleUtils.WriteMarkupContentAsHTML(result);
              plainText = false;
            }

          return new {Status = "OK", PlainText = plainText, Result = result};
        }
        catch(Exception error)
        {
          return new {Status = "Error", Msg = error.Message, Exception = error.ToMessageWithType()};
        }
      }

  }
}
