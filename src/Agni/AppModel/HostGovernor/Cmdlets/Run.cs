﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX;
using NFX.Environment;
using Agni.AppModel.Terminal;

namespace Agni.AppModel.HostGovernor.Cmdlets
{
    public class Run : Cmdlet
    {
        public const string CONFIG_CMD_ATTR = "cmd";
        public const string CONFIG_ARGS_ATTR = "args";
        public const string CONFIG_TIMEOUT_ATTR = "timeout";


        public Run(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
        {

        }


        public override string Execute()
        {
           var cmd = m_Args.AttrByName(CONFIG_CMD_ATTR).Value;
           if (cmd.IsNullOrWhiteSpace()) return "Missing 'cmd' - command to run";

           var args = m_Args.AttrByName(CONFIG_ARGS_ATTR).ValueAsString(string.Empty);

           var timeout = m_Args.AttrByName(CONFIG_TIMEOUT_ATTR).ValueAsInt(5000);

           if (timeout<0) return  "Timeout must be > 0 or blank";


           bool timedOut;
           var result = NFX.OS.ProcessRunner.Run(cmd, args, out timedOut, timeout);

           if (timedOut)
            result += "\n ....Process TIMED OUT....";

           return result;
        }

        public override string GetHelp()
        {
            return
@"Runs process blocking until it either exits or timeout expires.
           Parameters:
            <f color=yellow>cmd=string<f color=gray> - command to run
            <f color=yellow>args=int<f color=gray> - arguments
            <f color=yellow>timeout=int_ms<f color=gray> - for how long to wait for process exit
";
        }
    }
}
