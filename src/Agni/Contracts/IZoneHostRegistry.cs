﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX;
using NFX.Glue;
using NFX.Environment;
using NFX.Instrumentation;
using NFX.Log;
using NFX.OS;

using Agni.Metabase;

namespace Agni.Contracts
{
  /// <summary>
  /// Implemented by ZoneGovernors, receives host status/network identification data from subordinate nodes (another zone governors or other hosts).
  /// This contract is singleton for efficiency
  /// </summary>
  [Glued]
  [LifeCycle(ServerInstanceMode.Singleton)]
  public interface IZoneHostRegistry : IAgniService
  {

    /// <summary>
    /// Returns information for specified subordinate host/s name/s depending on hostName query parameter.
    /// Match pattern can contain up to one * wildcard and multiple ? wildcards
    /// </summary>
    IEnumerable<HostInfo> GetSubordinateHosts(string hostNameSearchPattern);

    /// <summary>
    /// Returns information for specified subordinate host or null
    /// </summary>
    HostInfo GetSubordinateHost(string hostName);

    /// <summary>
    /// Sends host registration/status update information from subordinate hosts
    /// </summary>
    void RegisterSubordinateHost(HostInfo host, DynamicHostID? hid = null);

    DynamicHostID Spawn(string hostPath, string id = null);
  }

  /// <summary>
  /// Client contract for IZoneHostRegistry svc
  /// </summary>
  public interface IZoneHostRegistryClient : IAgniServiceClient, IZoneHostRegistry
  {
    CallSlot Async_Spawn(string hostPath, string id = null);
  }

  /// <summary>
  /// Provides information about zone gov subordinate host
  /// </summary>
  [Serializable]
  public class HostInfo : INamed
  {
    private static int s_MyEntropyMs = 3 + (DateTime.UtcNow.Millisecond % 98);

    private HostInfo() { }

    public static HostInfo ForThisHost()
    {
      var cpu = Computer.CurrentProcessorUsagePct;
      var ram = Computer.GetMemoryStatus();
      var result = new HostInfo
      {
        m_Name = AgniSystem.HostName,//the HostName does not have spaces in dynamic host name
        m_UTCTimeStamp = App.TimeSource.UTCNow,

        m_LastWarning = App.Log.LastWarning.ThisOrNewSafeWrappedException(),
        m_LastError = App.Log.LastError.ThisOrNewSafeWrappedException(),
        m_LastCatastrophe = App.Log.LastCatastrophe.ThisOrNewSafeWrappedException(),

        m_CurrentCPULoad = cpu,
        m_CurrentRAMStatus = ram,
        m_NetInfo = HostNetInfo.ForThisHost(),

        m_SystemBuildInfo = BuildInformation.ForFramework,
        m_AgniBuildInfo = AgniSystem.CoreBuildInfo
      };


      if (result.UTCTimeStamp.Millisecond % s_MyEntropyMs == 0)
      {
        int entropy =
          unchecked(641 * cpu * ((int)ram.AvailablePhysicalBytes % 65171));
        //we purposely want to have "low" entropy so higher bytes are USUALLY 0

        NFX.ExternalRandomGenerator.Instance.FeedExternalEntropySample(entropy);
      }

      if (
          (cpu > 25 && cpu < 50 && (cpu & 0x1) == 0) || (cpu > 0 && cpu % 7 == 0)
         )
        result.m_RandomSample = NFX.ExternalRandomGenerator.Instance.NextRandomInteger;

      return result;
    }

    private string m_Name;
    private DateTime m_UTCTimeStamp;

    private int m_CurrentCPULoad;
    private MemoryStatus m_CurrentRAMStatus;

    private Message m_LastWarning;
    private Message m_LastError;
    private Message m_LastCatastrophe;

    private HostNetInfo m_NetInfo;

    private int? m_RandomSample;
    private BuildInformation m_SystemBuildInfo;
    private BuildInformation m_AgniBuildInfo;


    /// <summary>
    /// Host name (path) INCLUDING DYNAMIC HOST NAME suffix
    /// </summary>
    public string Name { get { return m_Name; } }

    /// <summary>
    /// UTC time stamp of the object status
    /// </summary>
    public DateTime UTCTimeStamp { get { return m_UTCTimeStamp; } }

    /// <summary>
    /// Returns last log warning message or null
    /// </summary>
    public Message LastWarning { get { return m_LastWarning; } }

    /// <summary>
    /// Returns last log error message or null
    /// </summary>
    public Message LastError { get { return m_LastError; } }

    /// <summary>
    /// Returns last log catastrophe message or null
    /// </summary>
    public Message LastCatastrophe { get { return m_LastCatastrophe; } }


    /// <summary>
    /// Current CPU load
    /// </summary>
    public int CurrentCPULoad { get { return m_CurrentCPULoad; } }

    /// <summary>
    /// Current memory status
    /// </summary>
    public MemoryStatus CurrentRAMStatus { get { return m_CurrentRAMStatus; } }


    /// <summary>
    /// Returns network information for host
    /// </summary>
    public HostNetInfo NetInfo { get { return m_NetInfo; } }


    /// <summary>
    /// Sometimes (randomly) captures random integer from ExternalRandomGenerator on the machine, or null
    /// </summary>
    public int? RandomSample { get { return m_RandomSample; } }

    /// <summary>
    /// Returns build info for system core (NFX or AFX...)
    /// </summary>
    public BuildInformation SystemBuildInfo { get { return m_SystemBuildInfo; } }

    /// <summary>
    /// Returns build info for Agni core
    /// </summary>
    public BuildInformation AgniBuildInfo { get { return m_AgniBuildInfo; } }
  }

  [Serializable]
  public struct DynamicHostID
  {
    public DynamicHostID(string id, string zone)
    {
      Zone = zone;
      ID = id;
    }

    public readonly string Zone;
    public readonly string ID;

    public override string ToString() { return "HID[{0}:{1}]".Args(ID, Zone); }
    public override int GetHashCode() { return ID.GetHashCode(); }
  }

  [Serializable]
  public sealed class DynamicHostInfo : INamed
  {
    public DynamicHostInfo(string id)
    {
      m_ID = id;
    }

    private string m_ID;

    public string ID { get { return m_ID; } }
    public DateTime Stamp { get; internal set; }
    public string Owner { get; internal set; }
    public string Host { get; internal set; }
    internal int Votes { get; set; }

    string INamed.Name { get { return m_ID; } }
  }
}
