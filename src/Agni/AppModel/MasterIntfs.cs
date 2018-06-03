using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX;
using NFX.ApplicationModel;
using NFX.Time;

using Agni.Identification;

namespace Agni.AppModel
{

    /// <summary>
    /// Defines rollef-up status per certain entity (i.e. Region, NOC, ZOne etc.)
    /// </summary>
    public interface IOperationalStatus
    {
       string Status { get; }

       int HostsTotal { get; }
       int ProcessorCoresUserUsedPct { get; }
       int ProcessorCoresSysUsedPct { get; }
       int ProcessorCoresTotal { get; }
       int MemoryGBUsed { get; }
       int MemoryGBTotal { get; }

       long Errors { get;}
    }

    /// <summary>
    /// Provides access to dynamic Agni status/operations.
    /// This Entity is similar to metabase, however unlike the metabse which is read-only configuration, this is a portal for working with Agni dynamic (changing)
    ///  information, such as: get statistics, broadcast messages etc.
    /// </summary>
    public interface IAgniSystem : IApplicationComponent, IDisposable
    {
       bool Available { get; }


       /// <summary>
       /// Returns the status of the system
       /// </summary>
       IOperationalStatus Status { get; }

       //todo In future add ability to get Zones,NOCS,Regions, their statitics etc...
    }


}
