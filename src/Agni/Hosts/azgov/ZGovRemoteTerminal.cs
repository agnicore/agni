using System;
using System.Collections.Generic;
using System.Linq;

using Agni.AppModel.Terminal;

namespace Agni.Hosts.azgov
{
  /// <summary>
  /// Implements Zone Governor remote terminal
  /// </summary>
  public class ZGovRemoteTerminal : AppRemoteTerminal
  {
    public ZGovRemoteTerminal() : base() { }

    public override IEnumerable<Type> Cmdlets
    {
      get
      {
        var local = CmdletFinder.FindByNamespace(typeof(ZGovRemoteTerminal), "Agni.Hosts.azgov.Cmdlets");
        return base.Cmdlets.Concat(CmdletFinder.ZGov).Concat(local);
      }
    }
  }
}