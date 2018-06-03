using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Agni.Dynamic
{
  /// <summary>
  /// Thrown to indicate workers related problems
  /// </summary>
  [Serializable]
  public class DynamicException : AgniException
  {
    public DynamicException() : base() {}
    public DynamicException(string message) : base(message) { }
    public DynamicException(string message, Exception inner) : base(message, inner) { }
    protected DynamicException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
