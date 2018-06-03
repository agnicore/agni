using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.ApplicationModel;
using NFX.Environment;
using NFX.IO.FileSystem;
using NFX;

namespace Agni.Metabase
{
  /// <summary>
  /// Provides shortcut access to mounted metabank file system
  /// </summary>
  public sealed class MetabankFileConfigNodeProvider : ApplicationComponent, IConfigNodeProvider
  {
    public MetabankFileConfigNodeProvider() : base(null)
    {
      m_Metabank = AgniSystem.Metabase;
    }

    private Metabank m_Metabank;

    [Config]
    public string File { get; set; }

    public void Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
    }

    public ConfigSectionNode ProvideConfigNode(object context = null)
    {
      if (File.IsNullOrWhiteSpace()) return null;

      return m_Metabank.GetConfigFromExistingFile(File).Root;
    }
  }
}
