using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX;
using NFX.Environment;

namespace Agni.MDB
{
  public abstract class ConnectStringBuilderBase : IConfigStringBuilder
  {
    [Config] public string Host;
    [Config] public string Network;
    [Config] public string Service;
    [Config] public string Binding;

    protected NFX.Glue.Node m_ResolvedNode;
    protected string m_ResolvedService;

    public abstract string BuildString();

    public void Configure(IConfigSectionNode node)
    {
      ConfigAttribute.Apply(this, node);
      m_ResolvedNode = AgniSystem.Metabase.ResolveNetworkService(Host, Network, Service, Binding);
    }
  }

  public sealed class MongoDBConnectStringBuilder : ConnectStringBuilderBase
  {
    [Config] public string DB;
    //todo specify more detailed parameters in future

    public override string BuildString()
    {
      return "mongo{{server='{0}:{1}' db='{2}'}}".Args(m_ResolvedNode.Host, m_ResolvedNode.Service, DB);
    }
  }

  public sealed class MySqlConnectStringBuilder : ConnectStringBuilderBase
  {
    [Config] public string DB;
    [Config] public string UserID;
    [Config] public string Password;
    [Config] public string ConnectionLifeTimeSec;
    //todo specify more detailed parameters in future

    public override string BuildString()
    {
      return "Server={0};Port={1};Database={2};Uid={3};Pwd={4};ConnectionLifeTime={5};".Args(m_ResolvedNode.Host, m_ResolvedNode.Service, DB, UserID, Password, ConnectionLifeTimeSec);
    }
  }
}
