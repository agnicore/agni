using System;

namespace Agni.AppModel.HostGovernor
{
  /// <summary>
  /// Implements contracts trampoline that uses a singleton instance of HostGovernorService
  /// </summary>
  public class HostGovernorServer 
    : Agni.Contracts.IHostGovernor,
      Agni.Contracts.IPinger
  {
    public Contracts.HostInfo GetHostInfo()
    {
      return HostGovernorService.Instance.GetHostInfo();
    }

    public void Ping()
    {
      HostGovernorService.Instance.Ping();
    }
  }
}
