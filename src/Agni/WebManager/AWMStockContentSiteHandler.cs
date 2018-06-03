using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX;
using NFX.Wave;
using NFX.Wave.Handlers;
using NFX.Environment;

namespace Agni.WebManager
{
  /// <summary>
  /// This handler serves the embedded content of Agni Web Manager site
  /// </summary>
  public class AWMStockContentSiteHandler : StockContentSiteHandler
  {
    public AWMStockContentSiteHandler(WorkDispatcher dispatcher, string name, int order, WorkMatch match)
                          : base(dispatcher, name, order, match){}


    public AWMStockContentSiteHandler(WorkDispatcher dispatcher, IConfigSectionNode confNode)
                          : base(dispatcher, confNode) {}


    public override string RootResourcePath
    {
      get { return "Agni.WebManager.Site"; }
    }
  }
}
