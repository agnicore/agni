using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using NFX;
using NFX.Environment;


namespace Agni.AppModel.Terminal.Cmdlets
{

    public class Mbc : Cmdlet
    {
        public Mbc(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
        {

        }

        public override string Execute()
        {
            if (!AgniSystem.IsMetabase)
             return "Metabase is not allocated";

            var result = new StringBuilder();
            AgniSystem.Metabase.DumpCacheStatus(result);

            return result.ToString();
        }

        public override string GetHelp()
        {
            return "Dumps status of metabase cache";
        }
   }
}
