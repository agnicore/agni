using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX;
using NFX.ServiceModel;
using NFX.Environment;

using Agni.AppModel;
using Agni.Metabase;

namespace Agni.Dynamic
{
  public class HostManager : ServiceWithInstrumentationBase<IAgniApplication>, IHostManagerImplementation
  {
    #region CONSTS
    private static readonly TimeSpan INSTRUMENTATION_INTERVAL = TimeSpan.FromMilliseconds(3700);
    #endregion

    #region .ctor
    public HostManager(IAgniApplication director) : base(director) { }

    protected override void Destructor()
    {
      DisposableObject.DisposeAndNull(ref m_InstrumentationEvent);
      base.Destructor();
    }
    #endregion

    #region Fields
    private bool m_InstrumentationEnabled;
    private NFX.Time.Event m_InstrumentationEvent;

    private NFX.Collections.NamedInterlocked m_Stats = new NFX.Collections.NamedInterlocked();
    #endregion

    #region Properties
    /// <summary>
    /// Implements IInstrumentable
    /// </summary>
    [Config(Default = false)]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_LOCKING, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
    public override bool InstrumentationEnabled
    {
      get { return m_InstrumentationEnabled; }
      set
      {
        m_InstrumentationEnabled = value;
        if (m_InstrumentationEvent == null)
        {
          if (!value) return;
          m_Stats.Clear();
          m_InstrumentationEvent = new NFX.Time.Event(App.EventTimer, null, e => AcceptManagerVisit(this, e.LocalizedTime), INSTRUMENTATION_INTERVAL);
        }
        else
        {
          if (value) return;
          DisposableObject.DisposeAndNull(ref m_InstrumentationEvent);
          m_Stats.Clear();
        }
      }
    }
    #endregion

    #region Public
    public Contracts.DynamicHostID Spawn(Metabank.SectionHost host, string id)
    {
      if (!host.Dynamic) throw new DynamicException("TODO");

      var hosts = host.ParentZone.ZoneGovernorHosts;
      return Contracts.ServiceClientHub.CallWithRetry<Contracts.IZoneHostRegistryClient, Contracts.DynamicHostID>
      (
        (controller) => controller.Spawn(host.RegionPath, id),
        hosts.Select(h => h.RegionPath)
      );
    }

    public string GetHostName(Contracts.DynamicHostID hid)
    {
      var zone = AgniSystem.Metabase.CatalogReg.NavigateZone(hid.Zone);
      var hosts = zone.ZoneGovernorHosts;
      return Contracts.ServiceClientHub.CallWithRetry<Contracts.IZoneHostReplicatorClient, Contracts.DynamicHostInfo>
      (
        (controller) => controller.GetDynamicHostInfo(hid),
        hosts.Select(h => h.RegionPath)
      ).Host;
    }
    #endregion
  }
}
