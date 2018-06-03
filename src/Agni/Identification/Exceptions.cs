using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Agni.Identification
{
  /// <summary>
  /// Thrown to indicate GDID generation related problems
  /// </summary>
  [Serializable]
  public class GDIDException : AgniException
  {
    public GDIDException() : base() { }
    public GDIDException(string message) : base(message) { }
    public GDIDException(string message, Exception inner) : base(message, inner) { }
    protected GDIDException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
