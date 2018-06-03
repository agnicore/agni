﻿using System;
using System.Collections.Generic;
using Agni.MDB;
using Agni.Social.Graph.Server.Data;
using Agni.Workers;
using NFX;
using NFX.DataAccess.CRUD;
using NFX.DataAccess.Distributed;

namespace Agni.Social.Graph.Server.Workers
{
  public abstract class EventSubscriptionBase : Todo
  {
    [Field(backendName: "g_own")] public GDID G_Owner { get; set; }
    [Field(backendName: "g_sub")] public GDID G_Subscriber { get; set; }

    protected virtual SubscriberRow FindSubscriber(out SubscriberVolumeRow minCntVol)
    {
      minCntVol = null;
      IEnumerable<SubscriberVolumeRow> volumes = GetSubscriberVolumes();
      SubscriberRow result = null;
      var i = 0;
      foreach (var volume in volumes)
      {
        var qry = Queries.FindSubscriber<SubscriberRow>(volume, G_Subscriber);
        if (result == null) result = ForNode(volume.G_SubscriberVolume).LoadRow(qry);
        if ( (volume.Count < SocialConsts.GetVolumeMaxCountForPosition(i++)) && (minCntVol == null || minCntVol.Count > volume.Count) ) minCntVol = volume;
      }
      return result;
    }

    protected virtual IEnumerable<SubscriberVolumeRow> GetSubscriberVolumes()
    {
      var qry = Queries.FindSubscriberVolumes<SubscriberVolumeRow>(G_Owner);
      return ForNode(G_Owner).LoadEnumerable(qry);
    }

    public CRUDOperations ForNode(GDID gNode)
    {
      return GraphOperationContext.Instance.DataStore.PartitionedOperationsFor(SocialConsts.MDB_AREA_NODE, gNode);
    }
  }
}