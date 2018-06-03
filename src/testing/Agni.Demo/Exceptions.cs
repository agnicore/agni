using System;
using System.Runtime.Serialization;

namespace Agni.Demo
{
  /// <summary>
  /// Thrown to indicate demo related problems
  /// </summary>
  [Serializable]
  public class DemoException : AgniException
  {
    public DemoException() : base() { }
    public DemoException(string message) : base(message) { }
    public DemoException(string message, Exception inner) : base(message, inner) { }
    protected DemoException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
