﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

using NFX;
using NFX.Log;
using NFX.Collections;
using NFX.ServiceModel;
using NFX.Environment;
using NFX.DataAccess.Distributed;
using NFX.ApplicationModel;

using Agni.Contracts;

namespace Agni.Identification
{
  /// <summary>
  /// Generates Global Distributed IDs - singleton, only one instance of this service may be allocated per process
  /// </summary>
  public sealed class GDIDAuthorityService : GDIDAuthorityServiceBase, IGDIDAuthority
  {
    #region Inner Classes
      private class scope : INamed
      {
        public string Name { get; set; }
        public Registry<sequence> Sequences = new Registry<sequence>();
      }

      internal class sequence : INamed
      {
        public string Name { get ; set; }
        public uint Era;
        public ulong Value;
        public bool New;
      }
    #endregion


    #region Static
      private static object s_InstanceLock = new object();
      private static volatile GDIDAuthorityService s_Instance;

      /// <summary>
      /// Returns singleton instance or throws if service has not been allocated yet
      /// </summary>
      public static GDIDAuthorityService Instance
      {
        get
        {
          var instance = s_Instance;
          if (instance==null)
            throw new GDIDException(StringConsts.GDIDAUTH_INSTANCE_NOT_ALLOCATED_ERROR);

          return instance;
        }
      }

    #endregion

    #region .ctor/.dctor
      /// <summary>
      /// Creates a singleton instance or throws if instance is already created
      /// </summary>
      public GDIDAuthorityService() : base()
      {
        lock(s_InstanceLock)
        {
          if (s_Instance!=null)
            throw new GDIDException(StringConsts.GDIDAUTH_INSTANCE_ALREADY_ALLOCATED_ERROR);

          s_Instance = this;
        }
      }

      protected override void Destructor()
      {
        lock(s_InstanceLock)
        {
          base.Destructor();
          s_Instance = null;
        }
      }
    #endregion

    #region Fields
      private byte[] m_AuthorityIDs;
      private Registry<scope> m_Scopes = new Registry<scope>();
    #endregion

    #region Properties

      public override string ComponentCommonName { get { return "gdida"; }}

      [Config("$"+CONFIG_AUTHORITY_IDS_ATTR)]
      public byte[] AuthorityIDs
      {
        get{ return m_AuthorityIDs; }
        set
        {
          CheckServiceInactive();

          if (value==null)
          {
            m_AuthorityIDs = null;
            return;
          }

          foreach(var id in value)
           if (id<0 || id>GDID.AUTHORITY_MAX)
             throw new GDIDException(StringConsts.GDIDAUTH_IDS_INVALID_AUTHORITY_VALUE_ERROR.Args(id));

          m_AuthorityIDs = value;
          Log(MessageType.Warning, "AuthorityIDs.set()", StringConsts.GDIDAUTH_AUTHORITY_ASSIGNMENT_WARNING.Args(value.ToDumpString(DumpFormat.Hex)));
        }
      }

    #endregion

    #region Public
      /// <summary>
      /// Performs block allocation
      /// </summary>
      public GDIDBlock AllocateBlock(string scopeName, string sequenceName, int blockSize, ulong? vicinity)
      {
        if (!Running)
          throw new GDIDException(StringConsts.GDIDAUTH_INSTANCE_NOT_RUNNING_ERROR);

        CheckNameValidity(scopeName);
        CheckNameValidity(sequenceName);

        if (blockSize<=0)
          throw new GDIDException(StringConsts.ARGUMENT_ERROR+"AllocateBlock(blockSize<=0)");

        scopeName = scopeName.ToUpperInvariant();//different cases for readability
        sequenceName = sequenceName.ToLowerInvariant();

        if (blockSize>MAX_BLOCK_SIZE) blockSize = MAX_BLOCK_SIZE;

        //get a subsequent authority index
        var now = DateTime.Now;
        var idx = (((now.DayOfYear * 24) + now.Hour) & CoreConsts.ABS_HASH_MASK) % this.m_AuthorityIDs.Length;
        byte authority = this.m_AuthorityIDs[idx];

        return allocate(authority, scopeName, sequenceName, blockSize, vicinity);
      }
    #endregion

    #region Protected
      protected override void DoStart()
      {
        if (m_AuthorityIDs==null || m_AuthorityIDs.Length<1)
           throw new GDIDException(StringConsts.GDIDAUTH_IDS_INVALID_AUTHORITY_VALUE_ERROR.Args("<no ids>"));
        base.DoStart();
      }
    #endregion

    #region .pvt

      private GDIDBlock allocate(byte authority, string scopeName, string sequenceName, int blockSize, ulong? vicinity)
      {
        var scopeKey = "{0}://{1}".Args(AuthorityPathSeg(authority), scopeName);

        var scope = m_Scopes.GetOrRegister(scopeKey,(key) => new scope{Name = key}, scopeKey);

        var sequence = scope.Sequences.GetOrRegister(sequenceName,(_) => new sequence{Name = sequenceName, New=true}, 0);//with NEW=TRUE

        var result = new GDIDBlock()
        {
           ScopeName = scopeName,
           SequenceName = sequenceName,
           Authority = authority,
           AuthorityHost = AgniSystem.HostName,
           BlockSize = blockSize,
           ServerUTCTime = App.TimeSource.UTCNow
        };

        lock(scope)
        {
          //0. If just allocated then need to read from disk
          if (sequence.New)
          {
            sequence.New = false;
            var id = ReadFromLocations(authority, scopeName, sequenceName);
            sequence.Era = id.Era;
            sequence.Value = id.Value;
          }

          //1. make a local copy of vars, that may mutate but dont get committed until written to disk
          var era = sequence.Era;
          var value = sequence.Value;

          //1.1 make sure that GDID.Zero is never returned
          if (authority==0 && era==0 && value==0) value = 1;//Don't start value from Zero in ERA=0 and Autority=0

          if (value >= GDID.COUNTER_MAX - (ulong)(blockSize + 1))//its time to update ERA (+1 for safeguard/edge case)
          {
             if (era==uint.MaxValue-4)//ALERT, with some room
             {
               Log(MessageType.CriticalAlert, "allocate()", StringConsts.GDIDAUTH_ERA_EXHAUSTED_ALERT.Args(scopeName, sequenceName));
             }
             if (era==uint.MaxValue) //hard stop
             {
               var txt = StringConsts.GDIDAUTH_ERA_EXHAUSTED_ERROR.Args(scopeName, sequenceName);
               Log(MessageType.CatastrophicError, "allocate()", txt);
               throw new GDIDException( txt );
             }

             era++;
             value = 0;
             Instrumentation.AuthEraPromotedEvent.Happened( scopeName, sequenceName );
             Log(MessageType.Warning, "allocate()", StringConsts.GDIDAUTH_ERA_PROMOTED_WARNING.Args(scopeName, sequenceName, era));
          }

          result.Era = era;
          result.StartCounterInclusive = value;

          value = value + (ulong)blockSize;

          //2. Try to write to disk, if it fails we could not allocate anything and will bail out with exception
          WriteToLocations(authority, scopeName, sequenceName, new _id(era, value));

          //3. only after write to disk succeeds do we commit the changes back into the sequence instance
          sequence.Era = era;
          sequence.Value = value;
        }

        return result;
      }
    #endregion

  }


}
