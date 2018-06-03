﻿using System;
using System.Collections.Generic;

using NFX;
using NFX.ApplicationModel;

namespace Agni.KDB
{
  public sealed partial class DefaultKDBStore
  {

    /// <summary>
    /// Base type for externally parametrized app components that are used throughout MDB implementation
    /// </summary>
    public abstract class KDBAppComponent : ApplicationComponent, IExternallyParameterized
    {
      protected KDBAppComponent(object director) : base(director)
      {

      }

      /// <summary>
      /// Returns named parameters that can be used to control this component
      /// </summary>
      public virtual IEnumerable<KeyValuePair<string, Type>> ExternalParameters { get { return ExternalParameterAttribute.GetParameters(this); } }

      /// <summary>
      /// Returns named parameters that can be used to control this component
      /// </summary>
      public virtual IEnumerable<KeyValuePair<string, Type>> ExternalParametersForGroups(params string[] groups)
      {
        return ExternalParameterAttribute.GetParameters(this, groups);
      }

      /// <summary>
      /// Gets external parameter value returning true if parameter was found
      /// </summary>
      public virtual bool ExternalGetParameter(string name, out object value, params string[] groups)
      {
        return ExternalParameterAttribute.GetParameter(this, name, out value, groups);
      }

      /// <summary>
      /// Sets external parameter value returning true if parameter was found and set
      /// </summary>
      public virtual bool ExternalSetParameter(string name, object value, params string[] groups)
      {
        return ExternalParameterAttribute.SetParameter(this, name, value, groups);
      }

    }

  }
}
