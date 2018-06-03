using System;
using System.Collections.Generic;
using System.Linq;

using Agni.AppModel.Terminal;

namespace Agni.Demo
{
  /// <summary>
  /// Implements Host Governor remote terminal
  /// </summary>
  public class DemoRemoteTerminal : AppRemoteTerminal
  {
    public DemoRemoteTerminal() : base() { }

    public override IEnumerable<Type> Cmdlets
    {
      get
      {
        var local = CmdletFinder.Find(typeof(DemoRemoteTerminal));
        return base.Cmdlets.Concat(local);
      }
    }
  }
}
