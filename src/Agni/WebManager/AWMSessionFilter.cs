using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Cryptography;


using NFX;
using NFX.Environment;
using NFX.Web;
using NFX.Wave;
using NFX.Wave.Filters;
using NFX.Serialization.JSON;


namespace Agni.WebManager
{
  /// <summary>
  /// Provides session management for AWM-specific sessions
  /// </summary>
  public sealed class AWMSessionFilter : SessionFilter
  {
    #region .ctor
      public AWMSessionFilter(WorkDispatcher dispatcher, string name, int order) : base(dispatcher, name, order) {}
      public AWMSessionFilter(WorkDispatcher dispatcher, IConfigSectionNode confNode): base(dispatcher, confNode) {ctor(confNode);}
      public AWMSessionFilter(WorkHandler handler, string name, int order) : base(handler, name, order) {}
      public AWMSessionFilter(WorkHandler handler, IConfigSectionNode confNode): base(handler, confNode) {ctor(confNode);}

      private void ctor(IConfigSectionNode confNode)
      {
        ConfigAttribute.Apply(this, confNode);
      }

    #endregion


      protected override WaveSession MakeNewSessionInstance(WorkContext work)
      {
        return new AWMWebSession(Guid.NewGuid());
      }

  }
}
