using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Agni.AppModel.HostGovernor
{
  /// <summary>
  /// Thrown to indicate AHGOV related problems
  /// </summary>
  [Serializable]
  public class AHGOVException : AgniException
  {
    public AHGOVException() : base() { }
    public AHGOVException(string message) : base(message) { }
    public AHGOVException(string message, Exception inner) : base(message, inner) { }
    protected AHGOVException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Thrown to indicate AHGOV ManagedApp-related problems
  /// </summary>
  [Serializable]
  public class ManagedAppException : AHGOVException
  {
    public ManagedAppException() : base() { }
    public ManagedAppException(string message) : base(message) { }
    public ManagedAppException(string message, Exception inner) : base(message, inner) { }
    protected ManagedAppException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
