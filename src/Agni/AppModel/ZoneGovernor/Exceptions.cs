using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Agni.AppModel.ZoneGovernor
{
  /// <summary>
  /// Thrown to indicate AZGOV related problems
  /// </summary>
  [Serializable]
  public class AZGOVException : AgniException
  {
    public AZGOVException() : base() { }
    public AZGOVException(string message) : base(message) { }
    public AZGOVException(string message, Exception inner) : base(message, inner) { }
    protected AZGOVException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
