using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agni.Dynamic;
using Agni.Workers;
using NFX.ApplicationModel;
using NFX.DataAccess;
using NFX.Environment;

namespace Agni.AppModel
{
  /// <summary>
  /// Represents an application that consists of pure-nop providers, consequently
  ///  this application does not log, does not store data and does not do anything else
  /// still satisfying its contract
  /// </summary>
  public class AgniNOPApplication : NOPApplication, IAgniApplication
  {
    private static AgniNOPApplication s_Instance = new AgniNOPApplication();

    protected AgniNOPApplication() : base() {}

    /// <summary>
    /// Returns a singlelton instance of the AgniNOPApplication
    /// </summary>
    public static new AgniNOPApplication Instance { get { return s_Instance; } }

    public string MetabaseApplicationName { get { return string.Empty; } }

    public IAgniSystem TheSystem { get { return NOPAgniSystem.Instance; } }

    public IConfigSectionNode BootConfigRoot { get { return m_Configuration.Root; } }

    public bool ConfiguredFromLocalBootConfig { get { return false; } }

    public SystemApplicationType SystemApplicationType { get { return SystemApplicationType.Unspecified; } }

    public Locking.ILockManager LockManager { get { return Locking.NOPLockManager.Instance; } }

    public IGDIDProvider GDIDProvider { get { throw new NotSupportedException("NOPApp.GDIDProvider"); } }

    public IProcessManager ProcessManager { get { throw new NotSupportedException("NOPApp.ProcessManager"); } }

    public IHostManager DynamicHostManager { get { throw new NotSupportedException("NOPApp.HostManager"); } }
  }
}
