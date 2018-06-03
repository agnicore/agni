using System;
using System.Collections.Generic;

using NFX.ApplicationModel;
using NFX.Environment;

namespace Agni.Log.Server
{
  /// <summary>
  /// Maps archive dimensions to/from model of the particular business system
  /// </summary>
  public class LogArchiveDimensionsMapper : ApplicationComponent
  {
    public LogArchiveDimensionsMapper(LogReceiverService director, IConfigSectionNode node) : base(director)
    {
      ConfigAttribute.Apply(this, node);
    }

    public virtual Dictionary<string, string> StoreMap(string archiveDimensions) { return null; }
    public virtual Dictionary<string, string> FilterMap(string archiveDimensions) { return null; }
  }
}
