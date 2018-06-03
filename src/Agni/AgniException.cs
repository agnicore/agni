using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using NFX;

namespace Agni
{
  /// <summary>
  /// Marker interfaces for all Agni exceptions
  /// </summary>
  public interface IAgniException
  {
  }

  /// <summary>
  /// Base exception thrown by the framework
  /// </summary>
  [Serializable]
  public class AgniException : NFXException, IAgniException
  {
    public const string SENDER_FLD_NAME = "AE-S";
    public const string TOPIC_FLD_NAME = "AE-T";

    public static string DefaultSender;
    public static string DefaultTopic;

    public readonly string Sender;
    public readonly string Topic;


    public AgniException()
    {
      Sender = DefaultSender;
      Topic = DefaultTopic;
    }

    public AgniException(int code)
    {
      Code = code;
      Sender = DefaultSender;
      Topic = DefaultTopic;
    }

    public AgniException(int code, string message) : this(message, null, code, null, null) {}
    public AgniException(string message) : this(message, null, 0, null, null) { }
    public AgniException(string message, Exception inner) : this(message, inner, 0, null, null) { }

    public AgniException(string message, Exception inner, int code, string sender, string topic) : base(message, inner)
    {
      Code = code;
      Sender = sender ?? DefaultSender;
      Topic = topic ?? DefaultTopic;
    }

    protected AgniException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
      Sender = info.GetString(SENDER_FLD_NAME);
      Topic = info.GetString(TOPIC_FLD_NAME);
    }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      if (info == null)
        throw new NFXException(StringConsts.ARGUMENT_ERROR + GetType().Name + ".GetObjectData(info=null)");
      info.AddValue(SENDER_FLD_NAME, Sender);
      info.AddValue(TOPIC_FLD_NAME, Topic);
      base.GetObjectData(info, context);
    }
  }
}
