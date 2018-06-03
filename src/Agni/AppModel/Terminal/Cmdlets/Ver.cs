using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Reflection;

using NFX;
using NFX.Environment;


namespace Agni.AppModel.Terminal.Cmdlets
{

    public class Ver : Cmdlet
    {
        public Ver(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
        {

        }

        public override string Execute()
        {
            var result = new StringBuilder(0xff);
            result.AppendLine("Server Version/Build information:");
            result.AppendLine(" App:     " + App.Name);
            result.AppendLine(" NFX:     " + BuildInformation.ForFramework);
            result.AppendLine(" Agni:    " + new BuildInformation( typeof(Agni.AgniSystem).Assembly ));
            result.AppendLine(" Host:    " + new BuildInformation( Assembly.GetEntryAssembly() ));

            return result.ToString();
        }

        public override string GetHelp()
        {
            return "Returns version/build information";
        }
    }

}
