﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX;

namespace Agni.AppModel.ZoneGovernor
{
  /// <summary>
  /// Implements contracts trampoline that uses a singleton instance of ZoneGovernorService
  /// </summary>
  public sealed class ZoneGovernorServer : Agni.Contracts.IZoneTelemetryReceiver,
                                           Agni.Contracts.IZoneLogReceiver,
                                           Agni.Contracts.IZoneHostRegistry,
                                           Agni.Contracts.IZoneHostReplicator,
                                           Agni.Contracts.ILocker
  {
    public int SendTelemetry(string host, NFX.Instrumentation.Datum[] data)
    {
      return ZoneGovernorService.Instance.SendTelemetry(host, data);
    }

    public int SendLog(string host, string appName, NFX.Log.Message[] data)
    {
      return ZoneGovernorService.Instance.SendLog(host, appName, data);
    }

    public Contracts.HostInfo GetSubordinateHost(string hostName)
    {
      return ZoneGovernorService.Instance.GetSubordinateHost(hostName);
    }

    public IEnumerable<Contracts.HostInfo> GetSubordinateHosts(string hostNameSearchPattern)
    {
      return ZoneGovernorService.Instance.GetSubordinateHosts(hostNameSearchPattern);
    }

    public void RegisterSubordinateHost(Contracts.HostInfo host, Contracts.DynamicHostID? hid)
    {
      ZoneGovernorService.Instance.RegisterSubordinateHost(host, hid);
    }

    public Contracts.DynamicHostID Spawn(string hostPath, string id = null)
    {
      return ZoneGovernorService.Instance.Spawn(hostPath, id);
    }

    public void PostDynamicHostInfo(Contracts.DynamicHostID hid, DateTime stamp, string owner, int votes)
    {
      ZoneGovernorService.Instance.PostDynamicHostInfo(hid, stamp, owner, votes);
    }

    public void PostHostInfo(Contracts.HostInfo host, Contracts.DynamicHostID? hid)
    {
      ZoneGovernorService.Instance.PostHostInfo(host, hid);
    }

    public Contracts.DynamicHostInfo GetDynamicHostInfo(Contracts.DynamicHostID hid)
    {
      return ZoneGovernorService.Instance.GetDynamicHostInfo(hid);
    }

    public Locking.LockTransactionResult ExecuteLockTransaction(Locking.Server.LockSessionData session, Locking.LockTransaction transaction)
    {
      return ZoneGovernorService.Instance.Locker.ExecuteLockTransaction(session, transaction);
    }

    public bool EndLockSession(Locking.LockSessionID sessionID)
    {
      return ZoneGovernorService.Instance.Locker.EndLockSession(sessionID);
    }
  }
}
