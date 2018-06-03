using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX;
using NFX.Environment;
using NFX.DataAccess;

using Agni.AppModel;
using Agni.Identification;
using Agni.Metabase;

namespace Agni
{
  /// <summary>
  /// Provides a shortcut access to app-global Agni context
  /// </summary>
  public static class AgniSystem
  {
    private static BuildInformation s_CoreBuildInfo;

    /// <summary>
    /// Returns BuildInformation object for the core agni assembly
    /// </summary>
    public static BuildInformation CoreBuildInfo
    {
      get
      {
        //multithreading: 2nd copy is ok
        if (s_CoreBuildInfo == null)
          s_CoreBuildInfo = new BuildInformation(typeof(AgniSystem).Assembly);

        return s_CoreBuildInfo;
      }
    }

    private static string s_MetabaseApplicationName;

    /// <summary>
    /// Every agni application MUST ASSIGN THIS property at its entry point ONCE. Example: void Main(string[]args){ AgniSystem.MetabaseApplicationName = "MyApp1";...
    /// </summary>
    public static string MetabaseApplicationName
    {
      get { return s_MetabaseApplicationName; }
      set
      {
        if (s_MetabaseApplicationName != null || value.IsNullOrWhiteSpace())
          throw new AgniException(StringConsts.METABASE_APP_NAME_ASSIGNMENT_ERROR);
        s_MetabaseApplicationName = value;
      }
    }


    /// <summary>
    /// Returns instance of agni application container that this AgniSystem services
    /// </summary>
    public static IAgniApplication Application
    {
      get { return (App.Instance as IAgniApplication) ?? (IAgniApplication)AgniNOPApplication.Instance; }
    }

    /// <summary>
    /// Denotes system application/process type that this app container has, i.e.:  HostGovernor, WebServer, etc.
    /// </summary>
    public static SystemApplicationType SystemApplicationType { get { return Application.SystemApplicationType; } }

    /// <summary>
    /// Returns current instance
    /// </summary>
    public static IAgniSystem Instance { get { return Application.TheSystem; } }

    /// <summary>
    /// Returns true when AgniSystem is active non-NOP instance
    /// </summary>
    public static bool Available { get { return Instance != null && Instance.Available; } }

    /// <summary>
    /// References application configuration root used to boot this application instance
    /// </summary>
    public static IConfigSectionNode BootConfigRoot { get { return Application.BootConfigRoot; } }

    /// <summary>
    /// Host name of this machine as determined at boot. This is a shortcut to Agni.AppModel.BootConfLoader.HostName
    /// </summary>
    public static string HostName { get { return BootConfLoader.HostName; } }


    /// <summary>
    /// True if this host is dynamic
    /// </summary>
    public static bool DynamicHost { get { return BootConfLoader.DynamicHost; } }


    /// <summary>
    /// Returns parent zone governor host name or null if this is the top-level host in Agni.
    /// This is a shortcut to Agni.AppModel.BootConfLoader.ParentZoneGovernorPrimaryHostName
    /// </summary>
    public static string ParentZoneGovernorPrimaryHostName { get { return BootConfLoader.ParentZoneGovernorPrimaryHostName; } }


    /// <summary>
    /// NOC name for this host as determined at boot
    /// </summary>
    public static string NOCName { get { return NOCMetabaseSection.Name; } }


    /// <summary>
    /// True when metabase is mounted!=null
    /// </summary>
    public static bool IsMetabase { get { return BootConfLoader.Metabase != null; } }


    /// <summary>
    /// Returns metabank instance that interfaces the metabase as determined at application boot.
    /// If metabase is null then exception is thrown. Use IsMetabase to test for null instead
    /// </summary>
    public static Metabank Metabase
    {
      get
      {
        var result = BootConfLoader.Metabase;

        if (result == null)
        {
          var trace = new System.Diagnostics.StackTrace(false);
          throw new AgniException(StringConsts.METABASE_NOT_AVAILABLE_ERROR.Args(trace.ToString()));
        }

        return result;
      }
    }

    /// <summary>
    /// Returns Metabank.SectionHost (metabase's information about this host)
    /// </summary>
    public static Metabank.SectionHost HostMetabaseSection { get { return Metabase.CatalogReg.NavigateHost(HostName); } }

    /// <summary>
    /// Returns Metabank.SectionNOC (metabase's information about the NOC this host is in)
    /// </summary>
    public static Metabank.SectionNOC NOCMetabaseSection { get { return HostMetabaseSection.NOC; } }


    /// <summary>
    /// Returns Agni distributed lock manager
    /// </summary>
    public static Locking.ILockManager LockManager { get { return Application.LockManager; } }

    /// <summary>
    /// References distributed GDID provider
    /// </summary>
    public static IGDIDProvider GDIDProvider { get { return Application.GDIDProvider; } }

    /// <summary>
    /// Returns Agni distributed process manager
    /// </summary>
    public static Workers.IProcessManager ProcessManager { get { return Application.ProcessManager; } }

    /// <summary>
    /// Returns Agni distributed dynamic host manager
    /// </summary>
    public static Dynamic.IHostManager DynamicHostManager { get { return Application.DynamicHostManager; } }
  }
}
