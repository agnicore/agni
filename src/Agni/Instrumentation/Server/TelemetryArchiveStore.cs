using System;

using NFX;
using NFX.Log;
using NFX.ApplicationModel;
using NFX.Environment;
using System.Collections.Generic;
using NFX.Instrumentation;

namespace Agni.Instrumentation.Server
{
  /// <summary>
  /// Represents a base for entities that archive instrumentation data
  /// </summary>
  public abstract class TelemetryArchiveStore : ApplicationComponent
  {
    protected TelemetryArchiveStore(TelemetryReceiverService director, IConfigSectionNode node) : base(director)
    {
      ConfigAttribute.Apply(this, node);
    }

    /// <summary>
    /// References service that this store is under
    /// </summary>
    public TelemetryReceiverService ArchiveService { get { return (TelemetryReceiverService)ComponentDirector;} }

    public abstract object BeginTransaction();

    public abstract void CommitTransaction(object transaction);

    public abstract void RollbackTransaction(object transaction);

    public abstract void Put(Datum[] data, object transaction);

    protected Guid Log(MessageType type,
                       string from,
                       string message,
                       Exception error = null,
                       Guid? relatedMessageID = null,
                       string parameters = null)
    {
      return ArchiveService.Log(type, "{0}.{1}".Args(GetType().Name, from), message, error, relatedMessageID, parameters);
    }
  }
}
