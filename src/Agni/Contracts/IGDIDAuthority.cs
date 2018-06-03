using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Glue;
using NFX.DataAccess.Distributed;
using Agni.Security.Permissions.Admin;


namespace Agni.Contracts
{
    /// <summary>
    /// Represents a Global Distributed ID Authority contract
    /// </summary>
    [Glued]
    [LifeCycle(ServerInstanceMode.Singleton)]
    public interface IGDIDAuthority : IAgniService
    {
        GDIDBlock AllocateBlock(string scopeName, string sequenceName, int blockSize, ulong? vicinity = GDID.COUNTER_MAX);
    }

    /// <summary>
    /// Contract for client for IGDIDAuthority service
    /// </summary>
    public interface IGDIDAuthorityClient : IAgniServiceClient, IGDIDAuthority
    {

    }


    /// <summary>
    /// Provides Global Distributed ID block allocated by authority
    /// </summary>
    [Serializable]
    public sealed class GDIDBlock
    {
        public string   ScopeName    { get; internal set;}
        public string   SequenceName { get; internal set;}
        public int      Authority    { get; internal set;}
        public string   AuthorityHost{ get; internal set;}
        public uint     Era          { get; internal set;}
        public ulong    StartCounterInclusive { get; internal set;}
        public int      BlockSize     { get; internal set;}
        public DateTime ServerUTCTime { get; internal set;}

        [NonSerialized]
        internal int __Remaining;//how much left per block
    }


    /// <summary>
    /// Represents a backup location where GDID Authority persists its data
    /// </summary>
    [Glued]
    [LifeCycle(ServerInstanceMode.Singleton)]
    public interface IGDIDPersistenceRemoteLocation : IAgniService
    {
        GDID? Read(byte authority, string sequenceName, string scopeName);
        void Write(string sequenceName, string scopeName, GDID value);
    }

    /// <summary>
    /// Contract for client for IGDIDPersistenceRemoteLocation service
    /// </summary>
    public interface IGDIDPersistenceRemoteLocationClient : IAgniServiceClient, IGDIDPersistenceRemoteLocation
    {

    }
}
