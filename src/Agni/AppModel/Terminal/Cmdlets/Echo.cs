using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX;
using NFX.ApplicationModel;
using NFX.Environment;
using NFX.Glue;
using Agni.Workers;

namespace Agni.AppModel.Terminal.Cmdlets
{
  /// <summary>
  /// Echo text
  /// </summary>
  public class Echo: Cmdlet
  {
    public Echo(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args) { }

    public override string Execute()
    {
      return m_Args.ValueAsString();
    }

    public override string GetHelp()
    {
      return @"Echo text";
    }
  }
}