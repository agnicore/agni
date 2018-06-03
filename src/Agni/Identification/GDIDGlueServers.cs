using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

using NFX;
using NFX.Log;
using NFX.Collections;
using NFX.ServiceModel;
using NFX.Environment;
using NFX.DataAccess.Distributed;
using NFX.ApplicationModel;

using Agni.Contracts;

namespace Agni.Identification
{
  /// <summary>
  /// Implements GDIDAuthority contract trampoline that uses a singleton instance of GDIDAuthorityService to
  ///  allocate blocks
  /// </summary>
  public sealed class GDIDAuthority : IGDIDAuthority
  {
    /// <summary>
    /// Implements IGDIDAuthority contract - allocates block
    /// </summary>
    public GDIDBlock AllocateBlock(string scopeName, string sequenceName, int blockSize, ulong? vicinity = GDID.COUNTER_MAX)
    {
      Instrumentation.AuthAllocBlockCalledEvent.Happened(scopeName, sequenceName);
      return GDIDAuthorityService.Instance.AllocateBlock(scopeName, sequenceName, blockSize, vicinity);
    }
  }

  /// <summary>
  /// Implements IGDIDPersistenceLocation contract trampoline that uses a singleton instance of GDIDPersistenceLocationService to
  ///  store gdids
  /// </summary>
  public sealed class GDIPersistenceRemoteLocation : IGDIDPersistenceRemoteLocation
  {
    public GDID? Read(byte authority, string sequenceName, string scopeName)
    {
      throw new NotImplementedException();
    }

    public void Write(string sequenceName, string scopeName, GDID value)
    {
      throw new NotImplementedException();
    }
  }


}
