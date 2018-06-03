﻿using System;

using NFX;
using NFX.IO;
using NFX.Environment;
using NFX.ApplicationModel;
using NFX.Security;
using NFX.Glue;
using NFX.Glue.Protocol;

using Agni.Clients;

namespace Agni.Tools.ascon
{
  public static class ProgramBody
  {
    public static void Main(string[] args)
    {
      try
      {
        run(args);
      }
      catch (Exception error)
      {
        ConsoleUtils.Error(error.ToMessageWithType());
        ConsoleUtils.Info("Exception details:");
        Console.WriteLine(error.ToString());

        if (error is RemoteException)
          TerminalUtils.ShowRemoteException((RemoteException)error);

        Environment.ExitCode = -1;
      }
    }

    static void run(string[] args)
    {
      using (var app = new ServiceBaseApplication(args, null))
      {
        var silent = app.CommandArgs["s", "silent"].Exists;
        if (!silent)
        {
          ConsoleUtils.WriteMarkupContent(typeof(ProgramBody).GetText("Welcome.txt"));

          ConsoleUtils.Info("Build information:");
          Console.WriteLine(" NFX:     " + BuildInformation.ForFramework);
          Console.WriteLine(" Agni: " + new BuildInformation(typeof(Agni.AgniSystem).Assembly));
          Console.WriteLine(" Tool:    " + new BuildInformation(typeof(ascon.ProgramBody).Assembly));
        }

        if (app.CommandArgs["?", "h", "help"].Exists)
        {
          ConsoleUtils.WriteMarkupContent(typeof(ProgramBody).GetText("Help.txt"));
          return;
        }


        var cred = app.CommandArgs["c", "cred"];
        var user = cred.AttrByName("id").Value;
        var pwd = cred.AttrByName("pwd").Value;

        if (user.IsNullOrWhiteSpace())
        {
          if (!silent) Console.Write("User ID: ");
          user = Console.ReadLine();
        }
        else
         if (!silent) ConsoleUtils.Info("User ID: " + user);

        if (pwd.IsNullOrWhiteSpace())
        {
          if (!silent) Console.Write("Password: ");
          pwd = ConsoleUtils.ReadPassword('*');
          Console.WriteLine();
        }
        else
         if (!silent) ConsoleUtils.Info("Password: <supplied>");


        var node = app.CommandArgs.AttrByIndex(0).ValueAsString("{0}://127.0.0.1:{1}".Args(SysConsts.APTERM_BINDING,
                                                                                           SysConsts.DEFAULT_HOST_GOV_APPTERM_PORT));

        if (new Node(node).Binding.IsNullOrWhiteSpace())
          node = "{0}://{1}".Args(SysConsts.APTERM_BINDING, node);

        if (new Node(node).Service.IsNullOrWhiteSpace())
          node = "{0}:{1}".Args(node, SysConsts.DEFAULT_HOST_GOV_APPTERM_PORT);

        var file = app.CommandArgs["f", "file"].AttrByIndex(0).Value;

        if (file.IsNotNullOrWhiteSpace())
        {
          if (!System.IO.File.Exists(file))
            throw new AgniException("File not found:" + file);
          if (!silent) ConsoleUtils.Info("Reading from file: " + file);
          file = System.IO.File.ReadAllText(file);
          if (!silent) ConsoleUtils.Info("Command text: " + file);
        }

        var txt = app.CommandArgs["t", "txt"].AttrByIndex(0).Value;

        if (txt.IsNotNullOrWhiteSpace())
        {
          if (!silent) ConsoleUtils.Info("Verbatim command text: " + txt);
        }

        var credentials = new IDPasswordCredentials(user, pwd);


        using (var client = new RemoteTerminal(node.ToResolvedServiceNode(true)))
        {
          client.Headers.Add(new AuthenticationHeader(credentials));

          var hinfo = client.Connect("{0}@{1}".Args(user, System.Environment.MachineName));
          if (!silent)
          {
            var c = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Connected. Use ';' at line end to submit statement, 'exit;' to disconnect");
            Console.WriteLine("Type 'help;' for edification or '<command> /?;' for command-specific help");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(hinfo.WelcomeMsg);
            Console.ForegroundColor = c;
          }

          if (txt.IsNotNullOrWhiteSpace() || file.IsNotNullOrWhiteSpace())
          {
            try
            {
              if (txt.IsNotNullOrWhiteSpace()) write(client.Execute(txt));
              if (file.IsNotNullOrWhiteSpace()) write(client.Execute(file));
            }
            catch (RemoteException remoteError)
            {
              TerminalUtils.ShowRemoteException(remoteError);
              Environment.ExitCode = -1;
            }
          }
          else
          {
            while (true)
            {
              if (!silent)
              {
                var c = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("{0}@{1}@{2}>".Args(hinfo.TerminalName, hinfo.AppName, hinfo.Host));
                Console.ForegroundColor = c;
              }
              var command = "";

              while (true)
              {
                var ln = Console.ReadLine();
                command += ln;
                if (ln.EndsWith(";")) break;
                if (!silent)
                {
                  var c = Console.ForegroundColor;
                  Console.ForegroundColor = ConsoleColor.White;
                  Console.Write(">");
                  Console.ForegroundColor = c;
                }
              }

              command = command.Remove(command.Length - 1, 1);

              if (command == "exit") break;

              string response = null;

              try
              {
                response = client.Execute(command);
              }
              catch (RemoteException remoteError)
              {
                TerminalUtils.ShowRemoteException(remoteError);
                continue;
              }
              write(response);
            }
          }

          var disconnectMessage = client.Disconnect();
          if (!silent)
            write(disconnectMessage);

        }
      }

    }//run

    private static void write(string content)
    {
      if (content == null) return;
      if (content.StartsWith(Agni.AppModel.Terminal.AppRemoteTerminal.MARKUP_PRAGMA))
      {
        content = content.Remove(0, Agni.AppModel.Terminal.AppRemoteTerminal.MARKUP_PRAGMA.Length);
        ConsoleUtils.WriteMarkupContent(content);
      }
      else
        Console.WriteLine(content);
    }
  }
}
