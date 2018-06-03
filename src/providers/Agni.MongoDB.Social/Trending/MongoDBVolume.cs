using System;
using System.Collections.Generic;
using System.Linq;

using NFX;
using NFX.DataAccess.MongoDB.Connector;
using NFX.Environment;
using NFX.Serialization.BSON;
using NFX.Serialization.JSON;
using NFX.Log;

using Agni.Social;
using Agni.Social.Trending;
using Agni.Social.Trending.Server;

namespace Agni.MongoDB.Social.Trending
{
  /// <summary>
  /// Implements trending data volume using Mongo DB
  /// </summary>
  public sealed class MongoDBVolume : Volume
  {
    #region CONST

    public const string CONFIG_MONGO_SECTION = "mongo";
    public const string QUERY_TRENDING = @"{'dt' : { '$gte' : '$$startDate','$lte' : '$$endDate'}}";

    #endregion

    #region .ctor

    public MongoDBVolume(TrendingSystemService director) : base(director)
    {
      foreach (var tEntity in TrendingHost.AllEntities)
        m_DBUpdates.Register( new DBUpdate(tEntity, TrendingHost.GetDimensionNamesForEntity(tEntity)) );
    }

    #endregion

    #region Fields

    private string m_ConnectionString;
    private Database m_Database;
    private readonly Registry<DBUpdate> m_DBUpdates = new Registry<DBUpdate>();


    #endregion

    #region Porperties

    public override bool InstrumentationEnabled { get; set; }

    #endregion

    #region Protected

    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);
      m_ConnectionString = ConfigStringBuilder.Build(node, CONFIG_MONGO_SECTION);
    }

    protected override void DoStart()
    {
      base.DoStart();
      if(m_ConnectionString.IsNullOrEmpty()) throw new MongoSocialException(StringConsts.DATABASE_NOT_CONFIGURED_ERROR);
      m_Database = MongoClient.DatabaseFromConnectString( m_ConnectionString );
    }

    protected override void DoWaitForCompleteStop()
    {
      DisposeAndNull(ref m_Database);
      base.DoWaitForCompleteStop();
    }

    protected override void DoDeleteOldData(DateTime deletePoint)
    {
      m_Database.Collections.ForEach((collection, i) =>
      {
        if (!Running) return;
        var deleteAt = RoundDatePerDetalization(deletePoint, DetalizationLevel);
        var query = new Query();
        var betweenDate = new BSONDocument();
        betweenDate.Set(new BSONDateTimeElement("$lte", deleteAt));
        query.Set(new BSONDocumentElement("dt", betweenDate));
        var deleteEntry = new DeleteEntry(query, DeleteLimit.None);
        var result = collection.Delete(deleteEntry);
        //Log(MessageType.Debug, this.GetType().Name +".DoDeleteOldDate", deleteAt.ToLongDateString() + "\n" +result.ToJSON(JSONWritingOptions.PrettyPrint));
      });
    }

    protected override void DoWriteGauge(SocialTrendingGauge gauge)
    {
      var date = RoundDatePerDetalization(App.TimeSource.UTCNow, DetalizationLevel);

      var update = m_DBUpdates[gauge.Entity];
      if (update==null)
        throw new MongoSocialException(StringConsts.TRENDING_GAUSE_UNKNOWN_ENTITY_ERROR.Args(gauge.Entity, nameof(DoWriteGauge)));

      update.G_Entity = gauge.G_Entity;
      update.G_Shard = gauge.G_Shard;
      update.DateTime = date;
      update.Value = gauge.Value;
      update.Count = gauge.Count;
      update.Dimensions = TrendingHost.MapGaugeDimensions(gauge.Entity, gauge.Dimensions);

      var collection = m_Database[gauge.Entity];
      var r = collection.Update( update.MongoUpdateEntry );
      //Log(NFX.Log.MessageType.Debug  , "MongoDBVolume.DoWriteGauge", gauge.Entity+ " : " + r.ToJSON(JSONWritingOptions.PrettyPrint));
    }

    protected override List<TrendingEntity> DoGetTreding(TrendingQuery query)
    {
      var result = new List<TrendingEntity>();

      IEnumerable<string> collections;
      if (query.EntityType.IsNotNullOrWhiteSpace())
      {
        if (!TrendingHost.HasEntity(query.EntityType)) return result;
        collections = new[] { query.EntityType };
      }
      else
        collections = TrendingHost.AllEntities;

      foreach (var collection in collections)
      {
        var sort = new BSONDocument().Set(new BSONInt32Element(DBConsts.FIELD_VALUE, -1));
        var qry = new Query();
        var betweenDate = new BSONDocument();
        betweenDate.Set(new BSONDateTimeElement("$gte", query.StartDate));
        betweenDate.Set(new BSONDateTimeElement("$lte", query.EndDate));
        qry.Set(new BSONDocumentElement("dt", betweenDate));
        if(query.DimensionFilter.IsNotNullOrEmpty() )
          TrendingHost.MapGaugeDimensions(collection, query.DimensionFilter)
                      .ForEach(pair => qry.Set(new BSONStringElement(pair.Key, pair.Value)));

        var find = new Query();
        find.Set(new BSONDocumentElement("$query", qry));
        find.Set(new BSONDocumentElement("$orderby", sort));

        var fetchBy = 1000;
        if (query.FetchCount < fetchBy)
          fetchBy = query.FetchCount;
        using (var cursor = m_Database[collection].Find(find , query.FetchStart, fetchBy))
        {
          foreach (var doc in cursor)
          {
            if (result.Count >= query.FetchCount) break;

            var doc_dt = doc[DBConsts.FIELD_DATETIME].ObjectValue;

            var dt = doc[DBConsts.FIELD_DATETIME].ObjectValue.AsDateTime();
            var dl = MapDetalizationToMinutes(DetalizationLevel);
            var gshr = RowConverter.GDID_BSONtoCLR((BSONBinaryElement)doc[DBConsts.FIELD_G_SHARD]);
            var gent = RowConverter.GDID_BSONtoCLR((BSONBinaryElement)doc[DBConsts.FIELD_G_ENTITY]);
            var val = doc[DBConsts.FIELD_VALUE].ObjectValue.AsULong();

            result.Add(new TrendingEntity(dt,
              dl,
              query.EntityType,
              gshr,
              gent,
              val
              )
            );
          }
        }
      }
      return result;
    }

    #endregion

    #region pvt


    #endregion

  }
}