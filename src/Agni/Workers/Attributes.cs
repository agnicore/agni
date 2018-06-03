﻿using System;
using System.Collections.Generic;

using NFX;

namespace Agni.Workers
{
  /// <summary>
  /// Provides information about the decorated Todo type: Queue name and assignes a globally-unique immutable type id
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public sealed class TodoQueueAttribute : GuidTypeAttribute
  {
    public TodoQueueAttribute(string queueName, string typeGuid) : base(typeGuid)
    {
      if (queueName.IsNullOrWhiteSpace())
        throw new WorkersException(StringConsts.ARGUMENT_ERROR + GetType().FullName + ".ctor(queueName=null|empty)");

      QueueName = queueName;
    }

    /// <summary>
    /// Provides the name of the Queue which will store and process the decorated type
    /// </summary>
    public readonly string QueueName;
  }

  /// <summary>
  /// Provides information about the decorated Process type: assignes a globally-unique immutable type id
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public sealed class ProcessAttribute : GuidTypeAttribute
  {
    public ProcessAttribute(string typeGuid) : base(typeGuid) {}

    public string Description { get; set; }
  }

  /// <summary>
  /// Provides information about the decorated Signal type: assignes a globally-unique immutable type id
  /// </summary>
  [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
  public sealed class SignalAttribute : GuidTypeAttribute
  {
    public SignalAttribute(string typeGuid) : base(typeGuid) { }
  }
}
