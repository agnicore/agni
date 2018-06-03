using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX;
using NFX.Glue;

namespace Agni.Metabase{ public sealed partial class Metabank{

  /// <summary>
  /// Kepos information during validation scan
  /// </summary>
  public class ValidationContext
  {

    public ValidationContext(IList<MetabaseValidationMsg> output)
    {
       Output = output;
       State = new Dictionary<string, object>( StringComparer.InvariantCultureIgnoreCase );
    }


    /// <summary>
    /// The output target (i.e. EventedList)
    /// </summary>
    public readonly IList<MetabaseValidationMsg> Output;

    /// <summary>
    /// Dictionary of variables that may be needed to retain state during validation
    /// </summary>
    public readonly Dictionary<string, object> State;

    public T StateAs<T>(string key) where T : class
    {
      object value;
      if (State.TryGetValue(key, out value))
        return value as T;

      return null;
    }
  }



}}
