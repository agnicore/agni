using System;
using System.Runtime.Serialization;

using NFX;

namespace Agni.KDB
{
  /// <summary>
  /// Thrown to indicate KDB-related problems
  /// </summary>
  [Serializable]
  public class KDBException : AgniException
  {
    public KDBException() : base() { }
    public KDBException(string message) : base(message) { }
    public KDBException(string message, Exception inner) : base(message, inner) { }
    protected KDBException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
