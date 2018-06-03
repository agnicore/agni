using System;

using NFX;
using NFX.Environment;

using Agni.AppModel.Terminal;
using Agni.Metabase;

namespace Agni.Demo.Cmdlets
{
  public class Spawn : Cmdlet
  {
    public const string CONFIG_HOST_ATTR = "host";
    public const string CONFIG_ID_ATTR = "id";

    public const int DEFAULT_AFTER_SEC = 60;

    public Spawn(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args)
    { }

    public override string Execute()
    {
      var metabasePath = m_Args.AttrByName(CONFIG_HOST_ATTR).ValueAsString();
      var id = m_Args.AttrByName(CONFIG_ID_ATTR).ValueAsString();

      var host = AgniSystem.Metabase.CatalogReg.NavigateHost(metabasePath) as Metabank.SectionHost;
      try
      {
        var dynhost = AgniSystem.DynamicHostManager.Spawn(host, id);
        return dynhost.ToString();
      }
      catch (Exception error)
      {
        return "ERROR: " + error.ToMessageWithType();
      }
    }

    public override string GetHelp()
    {
      return
@"Spawn dynamic host at path.
           Parameters:
            <f color=yellow>host=path<f color=gray> - metabase path of dynamic host
            <f color=yellow>id=name<f color=gray> - optional identity
";
    }
  }
}
