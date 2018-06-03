using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using NFX;

namespace Agni.Locking
{
  /// <summary>
  /// Thrown to indicate locking related problems
  /// </summary>
  [Serializable]
  public class LockingException : AgniException
  {
    public LockingException() : base() { }
    public LockingException(string message) : base(message) { }
    public LockingException(string message, Exception inner) : base(message, inner) { }
    protected LockingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
