using System;
using System.Runtime.Serialization;
using Agni.MDB;
using NFX;
using NFX.DataAccess.MongoDB.Connector;

namespace Agni.MongoDB.Social
{
  /// <summary>
  /// Thrown to indicate mongo-social related problems
  [Serializable]
  public class MongoSocialException : Agni.Social.SocialException
  {
    public MongoSocialException() : base()
    {
    }

    public MongoSocialException(string message) : base(message)
    {
    }

    public MongoSocialException(string message, Exception inner) : base(message, inner)
    {
    }

    protected MongoSocialException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
  }

}