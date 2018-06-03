﻿using System;
using System.Collections.Generic;
using System.Linq;

using NFX;
using NFX.Log;
using NFX.Wave.MVC;
using NFX.Serialization.JSON;
using NFX.Web;

using Agni.Metabase;

namespace Agni.WebManager.Controllers
{
  /// <summary>
  /// Provides Metabase access
  /// </summary>
  public sealed class TheSystem: AWMController
  {
    /// <summary>
    /// Navigates/redirects request to process "appName" at host with metabase path "metabasePath"
    /// </summary>
    [Action]
    public object Navigate(string metabasePath, string appName)
    {
      var svc = SysConsts.NETWORK_SVC_WEB_MANAGER_PREFIX + appName;

      try
      {
        var url = AgniSystem.Metabase.ResolveNetworkServiceToConnectString(metabasePath,
                  SysConsts.NETWORK_INTERNOC,
                  svc);

        return new Redirect(url);
      }
      catch
      {
        WorkContext.Response.StatusCode = WebConsts.STATUS_404;
        WorkContext.Response.StatusDescription = WebConsts.STATUS_404_DESCRIPTION;
        return "Web Manager service in application '{0}' on host '{1}' netsvc '{2}' is not available".Args(appName, metabasePath, svc);
      }
    }

    [Action]
    public object LoadLevel(string path, bool hosts = false)
    {
      return LoadLevelImpl(path, hosts);
    }

    internal static object LoadLevelImpl(string path, bool hosts = false)
    {
      IEnumerable<Metabank.SectionRegionBase> children = null;

      Metabank.SectionRegionBase section = null;
      if (path.IsNotNullOrWhiteSpace() && path != "/" && path != "\\")
      {
        section = AgniSystem.Metabase.CatalogReg[path];
        if (section == null) return NFX.Wave.SysConsts.JSON_RESULT_ERROR;

        if (section is Metabank.SectionRegion)
        {
          var region = (Metabank.SectionRegion)section;
          children = region.SubRegionNames.OrderBy(r => r).Select(r => (Metabank.SectionRegionBase)region.GetSubRegion(r))
            .Concat(region.NOCNames.OrderBy(n => n).Select(n => (Metabank.SectionRegionBase)region.GetNOC(n)));
        }
        else if (section is Metabank.SectionNOC)
        {
          var noc = (Metabank.SectionNOC)section;
          children = noc.ZoneNames.OrderBy(z => z).Select(z => noc.GetZone(z));
        }
        else if (section is Metabank.SectionZone)
        {
          var zone = (Metabank.SectionZone)section;
          children = zone.SubZoneNames.OrderBy(z => z).Select(z => (Metabank.SectionRegionBase)zone.GetSubZone(z));
          if (hosts)
            children = children.Concat(zone.HostNames.OrderBy(h => h).Select(h => (Metabank.SectionRegionBase)zone.GetHost(h)));
        }
        else
          return NFX.Wave.SysConsts.JSON_RESULT_ERROR;
      }
      else
      {
        children = AgniSystem.Metabase.CatalogReg.Regions;
      }

      return new
      {
        OK=true,
        path=path,
        myPath=AgniSystem.HostMetabaseSection.RegionPath,
        myPathSegs=AgniSystem.HostMetabaseSection.SectionsOnPath.Select(s => s.Name).ToArray(),
        children=makeChildren(children)
      };
    }

    internal static object makeChildren(IEnumerable<Metabank.SectionRegionBase> children)
    {
      var res = new List<JSONDataMap>();

      foreach (var child in children)
      {
        var d = new JSONDataMap();

        d["name"] = child.Name;
        d["path"] = child.RegionPath;
        d["me"] = child.IsLogicallyTheSame(AgniSystem.HostMetabaseSection);
        d["tp"] = child.SectionMnemonicType;
        d["descr"] = child.Description;

        var host = child as Metabank.SectionHost;
        if (host != null)
        {
          var isZGov = host.IsZGov;
          d["role"] = host.RoleName;
          d["dynamic"] = host.Dynamic;
          d["os"] = host.OS;
          d["apps"] = host.Role.AppNames.OrderBy(a => a).ToArray();
          d["isZGov"] = isZGov;
          d["myZGov"] = child.IsLogicallyTheSame(host.ParentZoneGovernorPrimaryHost());

          string adminURL = null;
          try
          {
            adminURL = AgniSystem.Metabase.ResolveNetworkServiceToConnectString(host.RegionPath,
              SysConsts.NETWORK_INTERNOC,
              isZGov ? SysConsts.NETWORK_SVC_ZGOV_WEB_MANAGER : SysConsts.NETWORK_SVC_HGOV_WEB_MANAGER);
          }
          catch(Exception ex)
          {
            log(MessageType.Error, "LoadLevel.makeLevel()", ex.ToMessageWithType(), ex);
          }

          d["adminURL"] = adminURL;
        }
        else
        {
          d["geo"] = new { lat = child.EffectiveGeoCenter.Lat, lng = child.EffectiveGeoCenter.Lng };
        }

        res.Add(d);
      }

      return res;
    }

    private static void log(MessageType tp, string from, string text, Exception error = null)
    {
      App.Log.Write(new Message
      {
        Type = tp,
        Topic = SysConsts.LOG_TOPIC_APP_MANAGEMENT,
        From = "{0}.{1}".Args(typeof(TheSystem).FullName, from),
        Text = text,
        Exception = error
      });
    }

  }
}
