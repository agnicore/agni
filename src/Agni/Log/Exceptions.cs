using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using NFX;

namespace Agni.Log
{
  /// <summary>
  /// Thrown to indicate log archive related problems
  /// </summary>
  [Serializable]
  public class LogArchiveException : AgniException
  {
    public LogArchiveException() : base() {}
    public LogArchiveException(string message) : base(message) {}
    public LogArchiveException(string message, Exception inner) : base(message, inner) { }
    protected LogArchiveException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
