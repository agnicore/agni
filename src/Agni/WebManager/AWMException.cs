using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using NFX;

namespace Agni.WebManager
{
  /// <summary>
  /// Base exception thrown by the WebManager site
  /// </summary>
  [Serializable]
  public class AWMException : AgniException
  {
    public AWMException(int code, string message) : this(message, null, code, null, null) { }
    public AWMException(string message) : this(message, null, 0, null, null) { }
    public AWMException(string message, Exception inner) : this(message, inner, 0, null, null) { }
    public AWMException(string message, Exception inner, int code, string sender, string topic) : base(message, inner, code, sender, topic) { }
    protected AWMException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
