﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using NFX;

namespace Agni.Social
{
  /// <summary>
  /// Base exception thrown by the social framework
  /// </summary>
  [Serializable]
  public class SocialException : AgniException
  {
    public SocialException() : base() { }
    public SocialException(int code) : base(code) { }
    public SocialException(int code, string message) : base(code, message) { }
    public SocialException(string message) : base(message) { }
    public SocialException(string message, Exception inner) : base(message, inner) { }
    public SocialException(string message, Exception inner, int code, string sender, string topic) : base(message, inner, code, sender, topic) { }
    protected SocialException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
