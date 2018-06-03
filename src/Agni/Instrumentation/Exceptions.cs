using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using NFX;

namespace Agni.Instrumentation
{
  /// <summary>
  /// Thrown to indicate log archive related problems
  /// </summary>
  [Serializable]
  public class TelemetryArchiveException : AgniException
  {
    public TelemetryArchiveException() : base() {}
    public TelemetryArchiveException(string message) : base(message) {}
    public TelemetryArchiveException(string message, Exception inner) : base(message, inner) { }
    protected TelemetryArchiveException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
