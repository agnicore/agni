using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using NFX;

namespace Agni.Workers
{
  /// <summary>
  /// Thrown to indicate workers related problems
  /// </summary>
  [Serializable]
  public class WorkersException : AgniException
  {
    public WorkersException() : base() {}
    public WorkersException(string message) : base(message) {}
    public WorkersException(string message, Exception inner) : base(message, inner) { }
    protected WorkersException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
