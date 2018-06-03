﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Glue;
using NFX.Instrumentation;


namespace Agni.Contracts
{

    /// <summary>
    /// Implemented by ZoneGovernors, receive telemetry data from subordinate nodes (another zone governors or other hosts).
    /// This contract is singleton for efficiency
    /// </summary>
    [Glued]
    [LifeCycle(ServerInstanceMode.Singleton)]
    public interface IZoneTelemetryReceiver : IAgniService
    {
       /// <summary>
       /// Sends telemetry batch from named subordinate host.
       /// Returns the receiver condition - a number of expected Datum instances in the next call.
       /// Keep in mind that a large number may be returned and Glue buffer limit may not be sufficient for large send, so
       /// impose a limit on the caller side (i.e. 200 max datum instances per call)
       /// The busier the receiver gets, the lower is the number. This is a form of throttling/flow control
       /// </summary>
       int SendTelemetry(string host, Datum[] data);
    }


    /// <summary>
    /// Contract for client of IZoneTelemetryReceiver svc
    /// </summary>
    public interface IZoneTelemetryReceiverClient : IAgniServiceClient, IZoneTelemetryReceiver {  }


}
