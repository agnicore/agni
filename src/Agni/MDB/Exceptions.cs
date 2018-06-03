using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using NFX;

namespace Agni.MDB
{
  /// <summary>
  /// Thrown to indicate MDB-related problems
  /// </summary>
  [Serializable]
  public class MDBException : AgniException
  {
    public MDBException() : base() { }
    public MDBException(string message) : base(message) { }
    public MDBException(string message, Exception inner) : base(message, inner) { }
    protected MDBException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
