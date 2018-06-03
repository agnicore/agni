using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX;
using NFX.Environment;


namespace Agni.AppModel.Terminal.Cmdlets
{

    public class Conf : Cmdlet
    {
        public Conf(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
        {

        }

        public override string Execute()
        {
            var conf = new LaconicConfiguration();
            conf.CreateFromNode( App.ConfigRoot );
            return conf.SaveToString();
        }

        public override string GetHelp()
        {
            return "Fetches current configuration tree";
        }
    }

}
