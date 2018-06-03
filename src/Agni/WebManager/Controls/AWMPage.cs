﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX;
using NFX.DataAccess.CRUD;
using NFX.Wave.Client;
using NFX.Serialization.JSON;


namespace Agni.WebManager.Controls
{
  /// <summary>
  /// All pages in Apex derive from this one
  /// </summary>
  public abstract class AWMPage : AWMTemplate
  {
    public const string DEFAULT_DESCRIPTION = "Manage Angi";

    public const string DEFAULT_KEYWORDS =
      "Angi, Management";

    public const string DEFAULT_VIEWPORT = "width=device-width, initial-scale=1.0, user-scalable=no";


    protected static readonly string FAVICON_PNG = SURI.Image("favicon.ita.196x196.png");
    protected static readonly string FAVICON_ICO = SURI.Image("favicon.ico");
    protected static readonly string BASE_CSS    = SURI.Style("base.css");
    protected static readonly string MASTER_CSS  = SURI.Style("master.css");
    protected static readonly string JQUERY_JS   = SURI.Stock("script/jquery-2.1.4.min.js");
    protected static readonly string WV_JS       = SURI.Stock("script/wv.js");
    protected static readonly string WV_GUI_JS   = SURI.Stock("script/wv.gui.js");
    protected static readonly string WV_CHART_JS = SURI.Stock("script/wv.chart.svg.js");
    protected static readonly string AWM_JS      = SURI.Script("awm.js");
    protected static readonly string MASTER_JS   = SURI.Script("master.js");


    private string m_Title;
    private string m_Description;
    private string m_Keywords;
    private string m_Viewport;

    public virtual string Title
    {
      get {return m_Title.IsNullOrWhiteSpace() ? (AgniSystem.MetabaseApplicationName + "@" + AgniSystem.HostName)
                                               : m_Title;}
      set {m_Title = value;}
    }

    public virtual string Description
    {
      get {return m_Description.IsNullOrWhiteSpace() ? DEFAULT_DESCRIPTION : m_Description;}
      set {m_Description = value;}
    }

    public virtual string Keywords
    {
      get {return m_Keywords.IsNullOrWhiteSpace() ? DEFAULT_KEYWORDS : m_Keywords;}
      set {m_Keywords = value;}
    }

    public virtual string Viewport
    {
      get {return m_Viewport.IsNullOrWhiteSpace() ? DEFAULT_VIEWPORT : m_Viewport;}
      set {m_Viewport = value;}
    }

    /// <summary>
    /// Outputs menu HREF for menu A with optional "selectedPage" class
    /// </summary>
    public void MenuHREF<TPage>(string uri)
    {
      if (this is TPage)
       Context.Response.Write("href='#' class='selectedPage'");
      else
      {
       Context.Response.Write("href='");
       Context.Response.Write(uri);
       Context.Response.Write("'");
      }
    }

    public string FormJSON(FormModel form, Exception validationError = null, string recID = null, string target = null)
    {
      var lang = Localizer.GetLanguage();
      return RecordModelGenerator.DefaultInstance.RowToRecordInitJSON(form, validationError, recID, target, lang).ToJSON();
    }

  }
}
