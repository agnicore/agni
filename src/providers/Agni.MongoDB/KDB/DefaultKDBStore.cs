using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX;
using NFX.ApplicationModel;
using NFX.DataAccess;
using NFX.DataAccess.CRUD;
using NFX.Environment;
using NFX.Log;
using NFX.ServiceModel;
using NFX.Serialization.BSON;

using Agni.MongoDB;
using System.Threading;
using NFX.Collections;

namespace Agni.KDB
{
  /// <summary>
  /// Proivdes default implementation of IKDBDataStore
  /// </summary>
  public sealed partial class DefaultKDBStore : ServiceWithInstrumentationBase<object>, IKDBDataStoreImplementation
  {
    private static readonly TimeSpan INSTR_INTERVAL = TimeSpan.FromMilliseconds(3250);
    public const string KDB_TARGET = "kdb";

    #region .ctor
      public DefaultKDBStore() : this(null, null) { }
      public DefaultKDBStore(string name, object director) : base(director)
      {
        Name = name.IsNullOrWhiteSpace() ? Guid.NewGuid().ToString() : name;
        m_Converter = new RowConverter();
      }

      protected override void Destructor()
      {
        DisposableObject.DisposeAndNull(ref m_InstrumentationEvent);
        base.Destructor();
      }
    #endregion

    #region Fields
      private ShardSet m_RootShardSet;
      internal readonly RowConverter m_Converter;
      private bool m_InstrumentationEnabled;
      private NFX.Time.Event m_InstrumentationEvent;

      private NamedInterlocked m_stat_GetHitCount = new NamedInterlocked();
      private NamedInterlocked m_stat_GetFallbackHitCount = new NamedInterlocked();
      private NamedInterlocked m_stat_GetMissCount = new NamedInterlocked();
      private NamedInterlocked m_stat_GetTouchCount = new NamedInterlocked();
      private NamedInterlocked m_stat_PutCount = new NamedInterlocked();
      private NamedInterlocked m_stat_DeleteHitCount = new NamedInterlocked();
      private NamedInterlocked m_stat_DeleteMissCount = new NamedInterlocked();
      private NamedInterlocked m_stat_DeleteFallbackCount = new NamedInterlocked();
      private NamedInterlocked m_stat_ErrorCount = new NamedInterlocked();
      private NamedInterlocked m_stat_MigrationCount = new NamedInterlocked();
    #endregion

    #region Props
      /// <summary>
      /// Implements IInstrumentable
      /// </summary>
      [Config(Default=false)]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
      public override bool InstrumentationEnabled
      {
        get { return m_InstrumentationEnabled;}
        set
        {
          m_InstrumentationEnabled = value;

          if (m_InstrumentationEvent==null)
          {
            if (!value) return;

            resetStats();
            m_InstrumentationEvent = new NFX.Time.Event(App.EventTimer, null, e => AcceptManagerVisit(this, e.LocalizedTime), INSTR_INTERVAL);
          }
          else
          {
            if (value) return;
            DisposableObject.DisposeAndNull(ref m_InstrumentationEvent);
          }
        }
      }

      public string TargetName{ get { return KDB_TARGET; } }

      StoreLogLevel IDataStoreImplementation.LogLevel
      {
        get { return this.LogLevel >= MessageType.Trace ? StoreLogLevel.Trace : StoreLogLevel.Debug; }
        set {}
      }

      [Config(Default = MessageType.Error)]
      [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA, CoreConsts.EXT_PARAM_GROUP_INSTRUMENTATION)]
      public MessageType LogLevel { get; set; }

      public ShardSet RootShardSet { get { return m_RootShardSet; } }
    #endregion

    #region Public
      public void TestConnection()
      {

      }


      public Row Get(string table, byte[] key)
      {
        var result = this.Get<Row>(table, key, false);
        return result.IsAssigned ? result.Value : null;
      }

      public KDBRecord<TRow> Get<TRow>(string table, byte[] key, bool dontToch = false) where TRow : Row
      {
        KDB.KDBConstraints.CheckTableName(table, "Get");
        KDB.KDBConstraints.CheckKey(key, "Get");

        var set = m_RootShardSet;
        while(set != null)
        {
          var shard = set.GetShardForKey(key);
          var result = shard.Get<TRow>(table, key);
          if (result.IsAssigned)
          {
            if (set.FallbackLevel == 0) incCtr(m_stat_GetHitCount, table);

            if (set.FallbackLevel > 0)
            {
              incCtr(m_stat_GetFallbackHitCount, table);
              try
              {
                incCtr(m_stat_MigrationCount, table);
                this.Put(table, key, result.Value, result.SlidingExpirationDays, result.AbsoluteExpirationDateUTC);
              }
              catch (Exception error)
              {
                incCtr(m_stat_ErrorCount, table);
                Log(MessageType.Error, "Get().Put(RootShardSet)", error.ToMessageWithType(), error);
              }
            }
            else if (!dontToch && ((App.TimeSource.UTCNow - result.LastUseDate).TotalDays > 1))
            {
              touchOnGet(shard, table, key);
            }
            return result;
          }

          set = set.Fallback;
        }
        incCtr(m_stat_GetMissCount, table);

        return KDBRecord<TRow>.Unassigned;
      }

      public KDBRecord<byte[]> GetRaw(string table, byte[] key, bool dontToch = false)
      {
        KDB.KDBConstraints.CheckTableName(table, "Get");
        KDB.KDBConstraints.CheckKey(key, "Get");

        var set = m_RootShardSet;
        while(set != null)
        {
          var shard = set.GetShardForKey(key);
          var result = shard.GetRaw(table, key);
          if (result.IsAssigned)
          {
            if (set.FallbackLevel == 0) incCtr(m_stat_GetHitCount, table);

            if (set.FallbackLevel > 0)
            {
              incCtr(m_stat_GetFallbackHitCount, table);
              try
              {
                incCtr(m_stat_MigrationCount, table);
                this.PutRaw(table, key, result.Value, result.SlidingExpirationDays, result.AbsoluteExpirationDateUTC);
              }
              catch (Exception error)
              {
                incCtr(m_stat_ErrorCount, table);
                Log(MessageType.Error, "Get().PutRaw(RootShardSet)", error.ToMessageWithType(), error);
              }
            }
            else if (!dontToch && ((App.TimeSource.UTCNow - result.LastUseDate).TotalDays > 1))
            {
              touchOnGet(shard, table, key);
            }
            return result;
          }

          set = set.Fallback;
        }
        incCtr(m_stat_GetMissCount, table);

        return KDBRecord<byte[]>.Unassigned;
      }

      public void Put(string table, byte[] key, Row value, int slidingExpirationDays = -1, DateTime? absoluteExpirationDateUtc = null)
      {
        if (value == null) throw new KDBException(StringConsts.ARGUMENT_ERROR + "DefaultKDBStore.Put(value=null)");
        KDB.KDBConstraints.CheckTableName(table, "Put");
        KDB.KDBConstraints.CheckKey(key, "Put");

        var shard = m_RootShardSet.GetShardForKey(key);
        shard.Put(table, key, value, slidingExpirationDays, absoluteExpirationDateUtc);

        incCtr(m_stat_PutCount, table);
      }

      public void PutRaw(string table, byte[] key, byte[] value, int slidingExpirationDays = -1, DateTime? absoluteExpirationDateUtc = null)
      {
        if (value == null) throw new KDBException(StringConsts.ARGUMENT_ERROR + "DefaultKDBStore.PutRaw(value=null)");
        KDB.KDBConstraints.CheckTableName(table, "Put");
        KDB.KDBConstraints.CheckKey(key, "Put");

        var shard = m_RootShardSet.GetShardForKey(key);
        shard.PutRaw(table, key, value, slidingExpirationDays, absoluteExpirationDateUtc);

        incCtr(m_stat_PutCount, table);
      }

      public bool Delete(string table, byte[] key)
      {
        KDB.KDBConstraints.CheckTableName(table, "Delete");
        KDB.KDBConstraints.CheckKey(key, "Delete");

        var result = false;
        var set = RootShardSet;
        while(set != null)
        {
          var shard = set.GetShardForKey(key);
          if (shard.Delete(table, key))
          {
            result = true;
            if (set.FallbackLevel == 0) incCtr(m_stat_DeleteHitCount, table);
            else incCtr(m_stat_DeleteFallbackCount, table);
          }
          set = set.Fallback;
        }
        if (!result) incCtr(m_stat_DeleteMissCount, table);

        return result;
      }
    #endregion

    #region Protected
      protected override void DoConfigure(IConfigSectionNode node)
      {
        base.DoConfigure(node);
        if (node == null || !node.Exists) return;

        m_RootShardSet = new ShardSet(this, node);
      }

      protected override void DoStart()
      {
        if (m_RootShardSet == null)
          throw new KDBException(StringConsts.KDB_STORE_ROOT_SHARDSET_NOT_CONFIGURED);
      }

      protected override void DoWaitForCompleteStop()
      {
        base.DoWaitForCompleteStop();
      }

      protected override void DoAcceptManagerVisit(Object manager,DateTime managerNow)
      {
        base.DoAcceptManagerVisit(manager,managerNow);
        if (!m_InstrumentationEnabled) return ;

        var instr = App.Instrumentation;
        if (!instr.Enabled) return;

        foreach(var elm in m_stat_GetHitCount.SnapshotAllLongs(0))         instr.Record(new Instrumentation.GetHitCount(elm.Key, elm.Value));
        foreach(var elm in m_stat_GetFallbackHitCount.SnapshotAllLongs(0)) instr.Record(new Instrumentation.GetFallbackHitCount(elm.Key, elm.Value));
        foreach(var elm in m_stat_GetMissCount.SnapshotAllLongs(0))        instr.Record(new Instrumentation.GetMissCount(elm.Key, elm.Value));
        foreach(var elm in m_stat_GetTouchCount.SnapshotAllLongs(0))       instr.Record(new Instrumentation.GetTouchCount(elm.Key, elm.Value));
        foreach(var elm in m_stat_PutCount.SnapshotAllLongs(0))            instr.Record(new Instrumentation.PutCount(elm.Key, elm.Value));
        foreach(var elm in m_stat_DeleteHitCount.SnapshotAllLongs(0))      instr.Record(new Instrumentation.DeleteHitCount(elm.Key, elm.Value));
        foreach(var elm in m_stat_DeleteFallbackCount.SnapshotAllLongs(0)) instr.Record(new Instrumentation.DeleteFallbackCount(elm.Key, elm.Value));
        foreach(var elm in m_stat_DeleteMissCount.SnapshotAllLongs(0))     instr.Record(new Instrumentation.DeleteMissCount(elm.Key, elm.Value));
        foreach(var elm in m_stat_ErrorCount.SnapshotAllLongs(0))          instr.Record(new Instrumentation.ErrorCount(elm.Key, elm.Value));
        foreach(var elm in m_stat_MigrationCount.SnapshotAllLongs(0))      instr.Record(new Instrumentation.MigrationCount(elm.Key, elm.Value));
      }

      internal void Log(MessageType tp, string from, string text, Exception error = null, Guid? related = null)
      {
        //if (InstrumentationEnabled && tp >= MessageType.Error) Interlocked.Increment(ref m_stat_WorkerLoggedError);
        if (tp < LogLevel) return;
        App.Log.Write(new Message
        {
          Type = tp,
          Topic = SysConsts.LOG_TOPIC_KDB,
          From = "{0}.{1}".Args(GetType().Name, from),
          Text = text,
          Exception = error,
          RelatedTo = related ?? Guid.Empty
        });
      }
    #endregion

    #region .pvt
      private int m_TouchCount;
      private void touchOnGet(Shard shard, string table, byte[] key)
      {
        var cnt = Interlocked.Increment(ref m_TouchCount);
        if (cnt > 64)
          touchOnGetCore(shard, table, key);
        else
          Task.Run(()=> { touchOnGetCore(shard, table, key); });
      }

      private void touchOnGetCore(Shard shard, string table, byte[] key)
      {
        try
        {
          incCtr(m_stat_GetTouchCount, table);
          shard.Touch(table, key);
        }
        catch(Exception error)
        {
          incCtr(m_stat_ErrorCount, table);
          Log(MessageType.Error, "touchOnGetCore()", error.ToMessageWithType(), error);
        }
        finally
        {
          Interlocked.Decrement(ref m_TouchCount);
        }
      }

      private void incCtr(NamedInterlocked counter, string table)
      {
        if (!m_InstrumentationEnabled) return;

        counter.IncrementLong(NFX.Instrumentation.Datum.UNSPECIFIED_SOURCE);
        if (table != null)
          counter.IncrementLong(table);
      }

      private void resetStats()
      {
        m_stat_GetHitCount.Clear();
        m_stat_GetFallbackHitCount.Clear();
        m_stat_GetMissCount.Clear();
        m_stat_GetTouchCount.Clear();
        m_stat_PutCount.Clear();
        m_stat_DeleteHitCount.Clear();
        m_stat_DeleteMissCount.Clear();
        m_stat_DeleteFallbackCount.Clear();
        m_stat_ErrorCount.Clear();
        m_stat_MigrationCount.Clear();
      }
    #endregion
  }
}
