using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX;
using NFX.Environment;


namespace Agni.AppModel.Terminal
{
    /// <summary>
    /// Provides generalizatin for commandlet - terminal command handler
    /// </summary>
    public abstract class Cmdlet : DisposableObject
    {
        protected Cmdlet(AppRemoteTerminal terminal, IConfigSectionNode args)
        {
            m_Terminal = terminal;
            m_Args = args;
        }

        protected AppRemoteTerminal m_Terminal;
        protected IConfigSectionNode m_Args;

        public abstract string Execute();

        public abstract string GetHelp();
    }
}
