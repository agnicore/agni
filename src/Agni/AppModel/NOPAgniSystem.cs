using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.ApplicationModel;

namespace Agni.AppModel
{
  public class NOPAgniSystem : ApplicationComponent, IAgniSystem
  {
    private static NOPAgniSystem s_Instance = new NOPAgniSystem();

    private NOPAgniSystem() { }

    public static NOPAgniSystem Instance { get { return s_Instance; } }

    public bool Available { get { return false; } }

    public override string ComponentCommonName { get { return AgniSystemBase.AGNI_COMPONENT_NAME; } }

    public IOperationalStatus Status
    {
      get
      {
        throw new NotImplementedException();
      }
    }
  }
}