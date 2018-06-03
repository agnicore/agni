using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Agni.Clients
{
  /// <summary>
  /// Thrown to indicate problems that arise while using clients (IAgniServiceClient implementors)
  /// </summary>
  [Serializable]
  public class AgniClientException : AgniException
  {
    public AgniClientException() : base() { }
    public AgniClientException(string message) : base(message) { }
    public AgniClientException(string message, Exception inner) : base(message, inner) { }
    protected AgniClientException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
