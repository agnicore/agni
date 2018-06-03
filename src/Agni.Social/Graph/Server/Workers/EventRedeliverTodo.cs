using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using Agni.Coordination;
using Agni.MDB;
using Agni.Social.Graph.Server.Data;
using Agni.Workers;

using NFX;
using NFX.DataAccess.CRUD;
using NFX.DataAccess.Distributed;
using NFX.Environment;
using NFX.Log;
using NFX.Serialization.Arow;

namespace Agni.Social.Graph.Server.Workers
{

  [Arow]
  [TodoQueue(SocialConsts.TQ_EVT_SUB_DELIVER, "A3410533-A9C4-47CD-A0DB-3980C13379F1")]
  public sealed class EventRedeliverTodo : EventDeliveryBase
  {

    [Field(backendName: "trd")]public SubscriberRow[] ToRedilever { get; set; }

    protected override void DoPrepareForEnqueuePostValidate(string targetName)
    {
      base.DoPrepareForEnqueuePostValidate(targetName);
      var key = G_Emitter.ToString();
      SysShardingKey = key;
      SysParallelKey = key;
    }

    protected override ExecuteState Execute(ITodoHost host, DateTime utcBatchNow)
    {
      try
      {
        IConfigSectionNode cfg = null;
        if (Event.Config.IsNotNullOrWhiteSpace()) cfg = Event.Config.AsLaconicConfig(handling: ConvertErrorHandling.ReturnDefault);
        var graphHost = GraphOperationContext.Instance.GraphHost;
        var badSubs = graphHost.DeliverEventsChunk(ToRedilever, Event, cfg);
        if (badSubs != null && badSubs.Any())
        {
          ToRedilever = badSubs.ToArray();
          return ExecuteState.ReexecuteUpdated;
        }
        return ExecuteState.Complete;
      }
      catch (Exception error)
      {
        host.Log(MessageType.Error, this, "Redeliver()", error.ToMessageWithType(), error);
      }
      return ExecuteState.ReexecuteAfterError;
    }

    protected override int RetryAfterErrorInMs(DateTime utcBatchNow)
    {
      if (SysTries < 2) return ExternalRandomGenerator.Instance.NextScaledRandomInteger(1000, 3000);
      if (SysTries < 5) return ExternalRandomGenerator.Instance.NextScaledRandomInteger(20000, 60000);
      if (SysTries <= 10) return ExternalRandomGenerator.Instance.NextScaledRandomInteger(80000, 160000);
      throw new SocialException("Event redeliver after failure {0} retries".Args(10));
    }

  }
}