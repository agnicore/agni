using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using NFX;

namespace Agni.WebMessaging
{
  /// <summary>
  /// Thrown to indicate web messaging problems
  /// </summary>
  [Serializable]
  public class WebMessagingException : AgniException
  {
    public WebMessagingException() : base() {}
    public WebMessagingException(string message) : base(message) {}
    public WebMessagingException(string message, Exception inner) : base(message, inner) { }
    protected WebMessagingException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
