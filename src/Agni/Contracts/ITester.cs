using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Glue;
using NFX.Log;

using Agni.Locking;
using Agni.Locking.Server;

namespace Agni.Contracts
{

    /// <summary>
    /// Used for various testing
    /// </summary>
    [Glued]
    [LifeCycle(ServerInstanceMode.Singleton)]
    public interface ITester : IAgniService
    {
       object TestEcho(object data);
    }


    /// <summary>
    /// Contract for client of ITester svc
    /// </summary>
    public interface ITesterClient : IAgniServiceClient, ITester
    {
       CallSlot Async_TestEcho(object data);
    }


}