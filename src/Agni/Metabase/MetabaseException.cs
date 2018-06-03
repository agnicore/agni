using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

using NFX;

namespace Agni.Metabase
{
  /// <summary>
  /// Thrown to indicate metabase-related problems
  /// </summary>
  [Serializable]
  public class MetabaseException : AgniException
  {
    public MetabaseException() : base() { }
    public MetabaseException(string message) : base(message) { }
    public MetabaseException(string message, Exception inner) : base(message, inner) { }
    protected MetabaseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
