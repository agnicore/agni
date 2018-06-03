﻿using System;
using System.Collections.Generic;
using System.Linq;

using NFX;
using NFX.DataAccess.CRUD;
using NFX.Environment;
using NFX.DataAccess.MongoDB.Connector;
using MongoQuery = NFX.DataAccess.MongoDB.Connector.Query;
using NFX.Serialization.BSON;

using Agni.MongoDB;

namespace Agni.KDB { public sealed partial class DefaultKDBStore {


  public const string CONFIG_SHARD_SECTION = "shard";
  public const string CONFIG_FALLBACK_SECTION = "fallback";
  public const string CONFIG_ORDER_ATTR = "order";
  public const string CONFIG_PRIMARY_CONNECT_STRING_ATTR = "primary-cs";
  public const string CONFIG_SECONDARY_CONNECT_STRING_ATTR = "secondary-cs";

  /// <summary>
  /// Represents partition within the area
  /// </summary>
  public sealed class ShardSet : KDBAppComponent
  {
    internal ShardSet(object director, IConfigSectionNode config) : base(director)
    {
      //Shards
      var shards = new List<Shard>();
      foreach(var snode in config.Children.Where( cn => cn.IsSameName(CONFIG_SHARD_SECTION)))
      {
        var shard = new Shard(this, snode);
        shards.Add( shard );
      }

      if (shards.Count==0)
        throw new KDBException(StringConsts.KDB_SHARDSET_NO_SHARDS_ERROR + config.RootPath);

      if (shards.Count != shards.Select(sh=>sh.Order).Distinct().Count())
        throw new KDBException(StringConsts.KDB_SHARDSET_DUPLICATE_SHARD_ORDER_ERROR + config.RootPath);

      shards.Sort();
      m_Shards = shards.ToArray();

      var nfb = config[CONFIG_FALLBACK_SECTION];
      if (nfb.Exists)
        m_Fallback = new ShardSet(this, nfb);
    }

    private Shard[] m_Shards;
    private ShardSet m_Fallback;

    public Shard[] Shards             { get{ return m_Shards; }}
    public ShardSet Fallback          { get{ return m_Fallback; }}

    public ShardSet FallbackParent    { get { return ComponentDirector as ShardSet; } }
    public int FallbackLevel          { get { return FallbackParent == null ? 0 : FallbackParent.FallbackLevel + 1; } }

    public DefaultKDBStore Store
    {
      get
      {
        var store = ComponentDirector as DefaultKDBStore;
        return store != null ? store : ((ShardSet)ComponentDirector).Store;
      }
    }

    /// <summary>
    /// Finds appropriate shard for key. See MDB.ShardingUtils
    /// </summary>
    public Shard GetShardForKey(byte[] key)
    {
      ulong subid = MDB.ShardingUtils.ObjectToShardingID(key);

      return Shards[ subid % (ulong)Shards.Length ];
    }

    public override string ToString()
    {
      return "ShardSet({0}, {1})".Args(m_Shards.Length, m_Fallback == null ? SysConsts.NULL : m_Fallback.ToString());
    }
  }//ShardSet




  /// <summary>
  /// Denotes connection types Primary/Secondary
  /// </summary>
  public enum ShardBackendConnection{Primary=0, Secondary}

  /// <summary>
  /// Represents a SHARD information for the DB particular host
  /// </summary>
  public sealed class Shard : KDBAppComponent, IComparable<Shard>
  {
    internal Shard(ShardSet set, IConfigSectionNode config) : base(set)
    {
      m_Order = config.AttrByName(CONFIG_ORDER_ATTR).ValueAsInt(0);

      PrimaryHostConnectString = ConfigStringBuilder.Build(config, CONFIG_PRIMARY_CONNECT_STRING_ATTR);
      SecondaryHostConnectString = ConfigStringBuilder.Build(config, CONFIG_SECONDARY_CONNECT_STRING_ATTR);

      if (PrimaryHostConnectString.IsNullOrWhiteSpace())
        throw new KDBException(StringConsts.KDB_SHARDSET_CONFIG_SHARD_CSTR_ERROR.Args(CONFIG_PRIMARY_CONNECT_STRING_ATTR, config.RootPath));

      if (SecondaryHostConnectString.IsNullOrWhiteSpace())
        throw new KDBException(StringConsts.KDB_SHARDSET_CONFIG_SHARD_CSTR_ERROR.Args(CONFIG_SECONDARY_CONNECT_STRING_ATTR, config.RootPath));
    }

    private int m_Order;
    private ShardBackendConnection  m_ConnectionType;

    public ShardSet ShardSet         { get{ return ComponentDirector as ShardSet; }}
    public DefaultKDBStore Store     { get{ return ShardSet.Store; }}
    public int Order                 { get{ return m_Order; }}


    public readonly string PrimaryHostConnectString;
    public readonly string SecondaryHostConnectString;


    /// <summary>
    /// Returns Primary then secondary connect strings
    /// </summary>
    public IEnumerable<string> ConnectStrings
    {
      get
      {
        yield return PrimaryHostConnectString;
        yield return SecondaryHostConnectString;
      }
    }


    /// <summary>
    /// Returns either primary or secondary connect string
    /// depending on connection type
    /// </summary>
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_DATA)]
    public ShardBackendConnection ConnectionType
    {
      get
      {
        return m_ConnectionType;
      }
      set
      {
        if (m_ConnectionType!=value)
        {
          m_ConnectionType = value;
          //todo Instrument
        }
      }
    }

    /// <summary>
    /// Returns either primary or secondary connect string
    /// depending on connection type
    /// </summary>
    public string EffectiveConnectionString
    {
      get
      {
        return m_ConnectionType==ShardBackendConnection.Primary ?
                PrimaryHostConnectString : SecondaryHostConnectString;
      }
    }

    public override string ToString()
    {
      return "Shard({0}, '{1}')".Args(m_Order, EffectiveConnectionString);
    }

    public int CompareTo(Shard other)
    {
      if (other == null) return -1;
      return this.Order.CompareTo(other.Order);
    }

    public const string FIELD_VALUE = "v";
    public const string FIELD_LAST_USE_DATE = "d";
    public const string FIELD_ABSOLUTE_EXPIRATION_DATEUTC = "a";
    public const string FIELD_SLIDING_EXPIRATION_DAYS = "s";


     /* BSON Document Schema:
      * ----------------
      *
      * {_id: key, v: {}|binary, lastUseDate: dateUTC, absoluteExpirationDateUTC: date|null, slidingExpirationDays: int|null}
      * actual field names:
      * {_id: key, v: {}|binary, d: dateUTC, a: date|null, s: int|null}
      */

    internal KDBRecord<TRow> Get<TRow>(string table, byte[] key) where TRow : Row
    {

      var db = MongoClient.DatabaseFromConnectString(EffectiveConnectionString);
      var doc = db[table].FindOne(MongoQuery.ID_EQ_BYTE_ARRAY(key));

      if (doc == null) return KDBRecord<TRow>.Unassigned;
      var elmValue = doc[FIELD_VALUE] as BSONDocumentElement;
      if (elmValue == null) return KDBRecord<TRow>.Unassigned;

      DateTime lastUseDate;
      DateTime? absoluteExpirationDateUTC;
      int slidingExpirationDays;
      readAttrs(table, doc, out lastUseDate, out absoluteExpirationDateUTC, out slidingExpirationDays);

      var value = elmValue.Value;

      TRow row;
      if (value == null)
      {
         row = Row.MakeRow(new Schema(Guid.NewGuid().ToString()), typeof(TRow)) as TRow;
      }
      else
      {
        var schema = Store.m_Converter.InferSchemaFromBSONDocument(value);
        row = Row.MakeRow(schema, typeof(TRow)) as TRow;
        Store.m_Converter.BSONDocumentToRow(value, row, null);
      }
      return new KDBRecord<TRow>(row, slidingExpirationDays, lastUseDate, absoluteExpirationDateUTC);
    }



    internal KDBRecord<byte[]> GetRaw(string table, byte[] key)
    {
      var db = MongoClient.DatabaseFromConnectString(EffectiveConnectionString);
      var doc = db[table].FindOne(MongoQuery.ID_EQ_BYTE_ARRAY(key));

      if (doc == null) return KDBRecord<byte[]>.Unassigned;
      var elmValue = doc[FIELD_VALUE] as BSONBinaryElement;
      if (elmValue == null) return KDBRecord<byte[]>.Unassigned;

      DateTime lastUseDate;
      DateTime? absoluteExpirationDateUTC;
      int slidingExpirationDays;
      readAttrs(table, doc, out lastUseDate, out absoluteExpirationDateUTC, out slidingExpirationDays);

      var value = elmValue.Value;

      return new KDBRecord<byte[]>(value.Data, slidingExpirationDays, lastUseDate, absoluteExpirationDateUTC);
    }


                private void readAttrs(string tbl, BSONDocument doc, out DateTime lastUse, out DateTime? absExp, out int sliding)
                {
                  var elmLastUse = doc[FIELD_LAST_USE_DATE] as BSONDateTimeElement;
                  if (elmLastUse==null)
                  {
                    Store.Log(NFX.Log.MessageType.Error, "GetX().readAttrs", "Table '{0}' DB doc has no '{1}'".Args(tbl, FIELD_LAST_USE_DATE));
                    lastUse = DateTime.MinValue;
                  }

                  lastUse = elmLastUse.Value;

                  var elmAbsExp = doc[FIELD_ABSOLUTE_EXPIRATION_DATEUTC];

                  if (elmAbsExp is BSONDateTimeElement)
                    absExp = ((BSONDateTimeElement)elmAbsExp).Value;
                  else
                    absExp = null;


                  var elmSE = doc[FIELD_SLIDING_EXPIRATION_DAYS];

                  if (elmSE is BSONInt32Element)
                   sliding = ((BSONInt32Element)elmSE).Value;
                  else
                   sliding = -1;
                }


    internal void Put(string table, byte[] key, Row value, int slidingExpirationDays, DateTime? absoluteExpirationDateUtc)
    {
      var elmValue = Store.m_Converter.RowToBSONDocumentElement(value, null, name: FIELD_VALUE);
      putCore(table, key, elmValue, slidingExpirationDays, absoluteExpirationDateUtc);
    }

    internal void PutRaw(string table, byte[] key, byte[] value, int slidingExpirationDays, DateTime? absoluteExpirationDateUtc)
    {
      var elmValue = RowConverter.ByteBuffer_CLRtoBSON(FIELD_VALUE, value);
      putCore(table, key, elmValue, slidingExpirationDays, absoluteExpirationDateUtc);
    }

    private void putCore(string table, byte[] key, BSONElement value, int slidingExpirationDays, DateTime? absoluteExpirationDateUtc)
    {
      var db = MongoClient.DatabaseFromConnectString(EffectiveConnectionString);

      var doc = new BSONDocument()
         .Set(RowConverter.ByteBufferID_CLRtoBSON(MongoQuery._ID, key))
         .Set(value)
         .Set(new BSONDateTimeElement(FIELD_LAST_USE_DATE, App.TimeSource.UTCNow))
         .Set(absoluteExpirationDateUtc.HasValue
                ? (BSONElement)new BSONDateTimeElement(FIELD_ABSOLUTE_EXPIRATION_DATEUTC, absoluteExpirationDateUtc.Value)
                : new BSONNullElement(FIELD_ABSOLUTE_EXPIRATION_DATEUTC))
         .Set(slidingExpirationDays > -1
                ? (BSONElement)new BSONInt32Element(FIELD_SLIDING_EXPIRATION_DAYS, slidingExpirationDays)
                : new BSONNullElement(FIELD_SLIDING_EXPIRATION_DAYS));

      db[table].Save(doc);
    }

    internal bool Delete(string table, byte[] key)
    {
      var db = MongoClient.DatabaseFromConnectString(EffectiveConnectionString);

      return db[table].DeleteOne(MongoQuery.ID_EQ_BYTE_ARRAY(key)).TotalDocumentsAffected > 0;
    }

    internal void Touch(string table, byte[] key)
    {
      var db = MongoClient.DatabaseFromConnectString(EffectiveConnectionString);
      var udoc = new BSONDocument()
                   .Set(new BSONDateTimeElement(FIELD_LAST_USE_DATE, App.TimeSource.UTCNow));

      // 20170404 spol: update document with $set to prevent document clear
      udoc = new BSONDocument()
               .Set(new BSONDocumentElement("$set", udoc));

      db[table].Update(new UpdateEntry(MongoQuery.ID_EQ_BYTE_ARRAY(key), udoc, multi: false, upsert: false));
    }
  }

  }
}
