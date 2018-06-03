using System;

using NFX;
using NFX.Environment;
using NFX.Web.Messaging;

using Agni.Contracts;

namespace Agni.WebMessaging
{
  /// <summary>
  /// Dispatches instances of AgniWebMessage into the remote IWebMessageSystem
  /// </summary>
  public class AgniWebMessageSink : MessageSink
  {
    #region CONSTS
    public const string CONFIG_HOST_ATTR = "host";

    #endregion

    public AgniWebMessageSink(MessageService director) : base(director)
    {
    }

    private string m_HostName;

    //todo: Refactor to use hosts sets with load balancing
    private IWebMessageSystemClient m_Client;


    /// <summary>
    /// Specifies the name of the host where the messages are sent
    /// </summary>
    [Config]
    [ExternalParameter(CoreConsts.EXT_PARAM_GROUP_MESSAGING)]
    public string Host { get; set; }

    public override MsgChannels SupportedChannels { get { return MsgChannels.All; } }


	protected override bool Filter(Message msg)
    {
      return true;
    }
	
    protected override void DoConfigure(IConfigSectionNode node)
    {
      base.DoConfigure(node);

      //throws on bad host spec
      AgniSystem.Metabase.CatalogReg.NavigateHost(Host);
    }

    protected override void DoWaitForCompleteStop()
    {
      base.DoWaitForCompleteStop();
      DisposeAndNull(ref m_Client);
    }


    protected override bool DoSendMsg(Message msg)
    {
      var amsg = msg as AgniWebMessage;
      if (amsg == null) return false;

      try
      {
        ensureClient();
        m_Client.SendMessage(amsg);
      }
      catch (Exception error)
      {
        throw new WebMessagingException("{0}.DoSend: {1}".Args(GetType().Name, error.ToMessageWithType()), error);
      }
      return true;
    }

    private void ensureClient()
    {
      var hn = this.Host;
      if (m_Client == null && !hn.EqualsOrdIgnoreCase(m_HostName))
      {
        m_Client = ServiceClientHub.New<IWebMessageSystemClient>(hn);
        m_HostName = hn;
      }
    }
  }
}
