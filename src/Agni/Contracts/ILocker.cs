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
    /// Implemented by ZoneGovernors, provide distributed lock manager services
    /// </summary>
    [Glued]
    [LifeCycle(ServerInstanceMode.Singleton)]
    public interface ILocker : IAgniService
    {
       LockTransactionResult ExecuteLockTransaction(LockSessionData session, LockTransaction transaction);
       bool EndLockSession(LockSessionID sessionID);
    }


    /// <summary>
    /// Contract for client of ILocker svc
    /// </summary>
    public interface ILockerClient : IAgniServiceClient, ILocker
    {
       CallSlot Async_ExecuteLockTransaction(LockSessionData session, LockTransaction transaction);
       CallSlot Async_EndLockSession(LockSessionID sessionID);
    }


}