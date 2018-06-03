using System;

using NFX;
using NFX.Environment;

using Agni.AppModel.Terminal;

namespace Agni.Demo.Cmdlets
{
  public class DynHost : Cmdlet
  {
    public const string CONFIG_ZONE_ATTR = "zone";
    public const string CONFIG_ID_ATTR = "id";

    public const int DEFAULT_AFTER_SEC = 60;

    public DynHost(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
    { }

    public override string Execute()
    {
      var zone = m_Args.AttrByName(CONFIG_ZONE_ATTR).ValueAsString();
      var id = m_Args.AttrByName(CONFIG_ID_ATTR).ValueAsString();

      try
      {
        var host = AgniSystem.DynamicHostManager.GetHostName(new Contracts.DynamicHostID(id, zone));
        return host;
      }
      catch (Exception error)
      {
        return "ERROR: " + error.ToMessageWithType();
      }
    }

    public override string GetHelp()
    {
      return
@"Get dynamic host name.
           Parameters:
            <f color=yellow>zone=path<f color=gray> - metabase path to zone
            <f color=yellow>id=name<f color=gray> - identity
";
    }
  }
}
