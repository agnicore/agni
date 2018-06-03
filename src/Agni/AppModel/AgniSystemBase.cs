using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX;
using NFX.ServiceModel;

namespace Agni.AppModel
{
  /// <summary>
  /// Provides base AgniSystem implementation
  /// </summary>
  public class AgniSystemBase : Service<IAgniApplication>, IAgniSystem
  {
    public const string AGNI_COMPONENT_NAME = "agni";

    #region .ctor
      internal AgniSystemBase(IAgniApplication app) : base(app)
      {

      }
    #endregion

    public override string ComponentCommonName { get { return AGNI_COMPONENT_NAME; } }

    public bool Available { get { return false; } }

    IOperationalStatus IAgniSystem.Status
    {
      get
      {
        throw new NotImplementedException();
      }
    }

    #region Protected
      protected override void DoStart()
      {
          base.DoStart();
      }

      protected override void DoWaitForCompleteStop()
      {
          base.DoWaitForCompleteStop();
      }
    #endregion
  }
}
