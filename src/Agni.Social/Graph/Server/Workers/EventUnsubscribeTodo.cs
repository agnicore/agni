using System;
using System.Collections.Generic;
using System.Resources;
using Agni.Coordination;
using Agni.Social.Graph.Server.Data;
using Agni.Workers;

using NFX;
using NFX.DataAccess.CRUD;
using NFX.DataAccess.Distributed;
using NFX.Log;
using NFX.Serialization.Arow;

namespace Agni.Social.Graph.Server.Workers
{

  [Arow]
  [TodoQueue(SocialConsts.TQ_EVT_SUB_MANAGE, "A166739E-6034-4390-B961-A9B51C858B80")]
  public sealed class EventUnsubscribeTodo : EventSubscriptionBase
  {
    protected override void DoPrepareForEnqueuePostValidate(string targetName)
    {
      base.DoPrepareForEnqueuePostValidate(targetName);
      var key = G_Owner.ToString();
      SysShardingKey = key;
      SysParallelKey = key;
    }

    protected override ExecuteState Execute(ITodoHost host, DateTime utcBatchNow)
    {
      try
      {
        SubscriberVolumeRow minColVol;
        var row = FindSubscriber(out minColVol);
        if (row == null  ) return ExecuteState.ReexecuteAfterError;
        ForNode(row.G_SubscriberVolume).Delete(row);
        return ExecuteState.Complete;
      }
      catch (Exception error)
      {
        host.Log(MessageType.Error, this, "Unsubscribe()", error.ToMessageWithType(), error);
        return ExecuteState.ReexecuteAfterError;
      }
    }

    protected override int RetryAfterErrorInMs(DateTime utcBatchNow)
    {
      if (SysTries < 2)   return ExternalRandomGenerator.Instance.NextScaledRandomInteger(1000, 3000);
      if (SysTries < 5)   return ExternalRandomGenerator.Instance.NextScaledRandomInteger(20000, 60000);
      if (SysTries <= 10) return ExternalRandomGenerator.Instance.NextScaledRandomInteger(80000, 160000);
      throw new GraphException("Event subscription after failure {0} retries".Args(10));
    }
  }
}