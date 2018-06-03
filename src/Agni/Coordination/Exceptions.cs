using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using NFX;

namespace Agni.Coordination
{
  /// <summary>
  /// Thrown to indicate coordination-related problems
  /// </summary>
  [Serializable]
  public class CoordinationException : AgniException
  {
    public CoordinationException() : base() { }
    public CoordinationException(string message) : base(message) { }
    public CoordinationException(string message, Exception inner) : base(message, inner) { }
    protected CoordinationException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
