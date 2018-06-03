using System;
using System.Collections.Generic;
using System.Linq;

using Agni.AppModel.Terminal;

namespace Agni.Hosts.ahgov
{
  /// <summary>
  /// Implements Host Governor remote terminal
  /// </summary>
  public class HGovRemoteTerminal : AppRemoteTerminal
  {
    public HGovRemoteTerminal() : base()
    {

    }

    public override IEnumerable<Type> Cmdlets
    {
      get
      {
        var local = CmdletFinder.FindByNamespace(typeof(HGovRemoteTerminal), "Agni.Hosts.ahgov.Cmdlets");
        return base.Cmdlets.Concat(CmdletFinder.HGov).Concat(local);
      }
    }
  }
}
