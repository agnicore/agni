using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Environment;
using NFX.ApplicationModel;
using NFX.DataAccess;

using Agni.Identification;

namespace Agni.AppModel
{

  /// <summary>
  /// Denotes system application/process types that this app container has, i.e.:  HostGovernor, WebServer, etc.
  /// </summary>
  public enum SystemApplicationType
  {
    Unspecified = 0,
    HostGovernor,
    ZoneGovernor,
    WebServer,
    GDIDAuthority,
    ServiceHost,
    ProcessHost,
    SecurityAuthority,
    TestRig,
    Tool
  }


  /// <summary>
  /// Defines a contract for applications
  /// </summary>
  public interface IAgniApplication : IApplication
  {
    /// <summary>
    /// Returns the name that uniquely identifies this application in the metabase. Every process/executable must provide its unique application name in metabase
    /// </summary>
    string MetabaseApplicationName { get; }

    /// <summary>
    /// References system-related functionality
    /// </summary>
    IAgniSystem TheSystem { get; }

    /// <summary>
    /// References application configuration root used to boot this application instance
    /// </summary>
    IConfigSectionNode BootConfigRoot { get; }

    /// <summary>
    /// Denotes system application/process type that this app container has, i.e.:  HostGovernor, WebServer, etc.
    /// </summary>
    SystemApplicationType SystemApplicationType { get; }

    /// <summary>
    /// References distributed lock manager
    /// </summary>
    Locking.ILockManager LockManager { get; }

    /// <summary>
    /// References distributed GDID provider
    /// </summary>
    IGDIDProvider GDIDProvider { get; }

    /// <summary>
    /// References distributed process manager
    /// </summary>
    Workers.IProcessManager ProcessManager { get; }

    /// <summary>
    /// References dynamic host manager
    /// </summary>
    Dynamic.IHostManager DynamicHostManager { get; }
  }
}
