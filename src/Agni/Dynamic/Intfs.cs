using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using NFX;
using NFX.ApplicationModel;
using NFX.Environment;
using NFX.Instrumentation;
using NFX.Log;

using Agni.Metabase;

namespace Agni.Dynamic
{
  public interface IHostManager
  {
    Contracts.DynamicHostID Spawn(Metabank.SectionHost host, string id);
    string GetHostName(Contracts.DynamicHostID hid);
  }

  public interface IHostManagerImplementation : IHostManager, IApplicationComponent, IDisposable, IConfigurable, IInstrumentable
  {
  }
}
