﻿using System;
using System.Reflection;
using System.Collections.Generic;

using NFX.Glue;
using NFX.Glue.Protocol;
using NFX.Log;

namespace Agni.Contracts
{

  /// <summary>
  /// Implemented by ILogReceiver, receive log data.
  /// This contract is singleton for efficiency
  /// </summary>
  [Glued]
  [LifeCycle(ServerInstanceMode.Singleton)]
  public interface ILogReceiver : IAgniService
  {
    [OneWay]
    [ArgsMarshalling(typeof(RequestMsg_ILogReceiver_SendLog))]
    void SendLog(Message data);

    Message GetByID(Guid id, string channel = null);

    IEnumerable<Message> List(string archiveDimensionsFilter, DateTime startDate, DateTime endDate, MessageType? type = null,
      string host = null, string channel = null, string topic = null,
      Guid? relatedTo = null,
      int skipCount = 0);
  }

  /// <summary>
  /// Contract for client of ILogReceiver svc
  /// </summary>
  public interface ILogReceiverClient : IAgniServiceClient, ILogReceiver
  {
    CallSlot Async_SendLog(Message data);
    CallSlot Async_GetByID(Guid id, string channel = null);
    CallSlot Async_List(string archiveDimensionsFilter, DateTime startDate, DateTime endDate, MessageType? type = null,
      string host = null, string channel = null, string topic = null,
      Guid? relatedTo = null,
      int skipCount = 0);
  }

  public sealed class RequestMsg_ILogReceiver_SendLog : RequestMsg
  {
    public RequestMsg_ILogReceiver_SendLog(MethodInfo method, Guid? instance) : base(method, instance) { }
    public RequestMsg_ILogReceiver_SendLog(TypeSpec contract, MethodSpec method, bool oneWay, Guid? instance) : base(contract, method, oneWay, instance) { }

    public Message MethodArg_0_data;
  }
}