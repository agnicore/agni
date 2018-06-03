using System;
using System.Collections.Generic;
using System.Linq;
using NFX;
using NFX.DataAccess.Distributed;
using NFX.DataAccess.MongoDB.Connector;
using NFX.RelationalModel;
using NFX.Serialization.BSON;

namespace Agni.MongoDB.Social.Trending
{
  /// <summary>
  /// Caches the pre-created MONGO DB update document, so it does not ge re-created from scratch with every insert
  /// This class is NOT thread safe
  /// </summary>
  internal sealed class DBUpdate : INamed
  {

    public DBUpdate(string entityType, string[] dimensionNames)
    {
      m_EntityType = entityType;

      m_Filter.Set(m_G_Entity);
      m_Filter.Set(m_G_Shard);
      m_Filter.Set(m_DateTime);

      m_Doc.Set(m_G_Entity);
      m_Doc.Set(m_G_Shard);
      m_Doc.Set(m_DateTime);

      foreach (var dimensionName in dimensionNames)
      {
        var element = new BSONStringElement(dimensionName, "");
        var kvp = new KeyValuePair<string, BSONStringElement>(dimensionName, element);
        m_Dimensions.Add(kvp);
        m_Doc.Set(element);
        m_Filter.Set(element);
      }

      m_Inc.Set(m_Value);
      m_Inc.Set(m_Count);

      m_IncDoc.Set(new BSONDocumentElement("$setOnInsert", m_Doc));
      m_IncDoc.Set(new BSONDocumentElement("$inc", m_Inc));

      m_MongoUpdateEntry = new UpdateEntry(m_Filter, m_IncDoc, false, true);
    }

    private readonly string m_EntityType;
    private readonly UpdateEntry m_MongoUpdateEntry;

    private readonly BSONDocument m_Filter = new BSONDocument();
    private readonly BSONDocument m_Doc = new BSONDocument();
    private readonly BSONDocument m_IncDoc = new BSONDocument();
    private readonly BSONDocument m_Inc = new BSONDocument();

    private readonly BSONBinaryElement m_G_Entity = new BSONBinaryElement(DBConsts.FIELD_G_ENTITY, new BSONBinary());
    private readonly BSONBinaryElement m_G_Shard = new BSONBinaryElement(DBConsts.FIELD_G_SHARD, new BSONBinary());
    private readonly BSONDateTimeElement m_DateTime = new BSONDateTimeElement(DBConsts.FIELD_DATETIME, DateTime.MinValue);
    private readonly List<KeyValuePair<string, BSONStringElement>> m_Dimensions = new  List<KeyValuePair<string, BSONStringElement>>();

    private readonly BSONInt64Element m_Value = new BSONInt64Element(DBConsts.FIELD_VALUE, 0);
    private readonly BSONInt64Element m_Count = new BSONInt64Element(DBConsts.FIELD_COUNT, 0);

    public string Name { get{ return m_EntityType; } }

    public UpdateEntry MongoUpdateEntry { get{ return m_MongoUpdateEntry; } }

    public GDID G_Entity
    {
      get { return RowConverter.GDID_BSONtoCLR(m_G_Entity); }
      set { m_G_Entity.Value = RowConverter.GDID_CLRtoBSONBinary(value); }
    }

    public GDID G_Shard
    {
      get { return RowConverter.GDID_BSONtoCLR(m_G_Shard); }
      set { m_G_Shard.Value = RowConverter.GDID_CLRtoBSONBinary(value); }
    }

    public DateTime DateTime
    {
      get { return m_DateTime.ObjectValue.AsDateTime(); }
      set { m_DateTime.Value = value; }
    }

    public long Value {
      get { return m_Value.ObjectValue.AsLong(); }
      set { m_Value.Value = value; }
    }

    public long Count {
      get { return m_Count.ObjectValue.AsLong(); }
      set { m_Count.Value = value; }
    }

    public IEnumerable<KeyValuePair<string, string>> Dimensions
    {
      get
      {
        var result = new List<KeyValuePair<string, string>>();
        foreach (var item in m_Dimensions)
        {
          var element = item.Value;
          var key = item.Key;
          var kvp = new KeyValuePair<string, string>(key, element.ObjectValue.ToString());
          result.Add(kvp);
        }
        return result;
      }
      set
      {
        foreach (var dimKVP in m_Dimensions)
        {
          var kvp = value.FirstOrDefault(pair => pair.Key.EqualsOrdIgnoreCase(dimKVP.Key));
          dimKVP.Value.Value = kvp.Value ?? "";
        }
      }

    }

  }
}