﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX;
using NFX.Environment;
using Agni.AppModel.Terminal;

namespace Agni.AppModel.HostGovernor.Cmdlets
{
    public class Install : Cmdlet
    {
        public const string CONFIG_FORCE_ATTR = "force";

        public Install(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
        {

        }

        public override string Execute()
        {
           var list = new List<string>();
           var force =  m_Args.AttrByName(CONFIG_FORCE_ATTR).ValueAsBool(false);

           if (force)
            App.Log.Write( new NFX.Log.Message
              {
                 Type = NFX.Log.MessageType.Warning,
                 Topic = SysConsts.LOG_TOPIC_APP_MANAGEMENT,
                 From = "{0}.Force".Args(GetType().FullName),
                 Text = "Installation with force=true initiated"
              });

           var anew = HostGovernorService.Instance.CheckAndPerformLocalSoftwareInstallation(list, force);

           var progress = list.Aggregate(new StringBuilder(), (sb, s) => sb.AppendLine(s)).ToString();

            App.Log.Write( new NFX.Log.Message
              {
                 Type = NFX.Log.MessageType.Warning,
                 Topic = SysConsts.LOG_TOPIC_APP_MANAGEMENT,
                 From = "{0}.Force".Args(GetType().FullName),
                 Text = "Installation finished. Installed anew: " + anew,
                 Parameters = progress
              });

           return progress;
        }

        public override string GetHelp()
        {
            return
@"Initiates check and installation of local software.
           Parameters:
            <f color=yellow>force=bool<f color=gray> - force reinstall
";
        }
    }
}
