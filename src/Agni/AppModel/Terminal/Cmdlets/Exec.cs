﻿using System;
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
  /// Execute commands
  /// </summary>
  public class Exec: Cmdlet
  {
    public const string CONFIG_TO_ATTR = "to";

    public Exec(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args) { }

    public override string Execute()
    {
      var to = m_Args.AttrByName(CONFIG_TO_ATTR).ValueAsString();
      var sb = new StringBuilder();
      foreach (var arg in m_Args.Children)
        sb.Append(m_Terminal.Execute(arg));
      if (to.IsNullOrWhiteSpace())
        return sb.ToString();
      m_Terminal.Vars[to] = sb.ToString();
      return "OK";
    }

    public override string GetHelp()
    {
      return @"Execute commands";
    }
  }
}