using System;
using System.Linq;
using System.Text;

using NFX;
using NFX.Web;
using NFX.Environment;
using NFX.IO.FileSystem.S3.V4;
using SVN_CONN_PARAMS = NFX.IO.FileSystem.SVN.SVNFileSystemSessionConnectParams;
using STRIPE_CONN_PARAMS = NFX.Web.Pay.Stripe.StripeConnectionParameters;

namespace WinFormsTest.Pay
{
  public class ExternalCfg
  {
    #region LACONF

      protected string LACONF = @"nfx 
{
  starters
  {
    starter
    {
      name='Payment Processing 1'
      type='NFX.Web.Pay.PaySystemStarter, NFX.Web'
      application-start-break-on-exception=true
    }

    starter
    {
      name='Social Processing'
      type='NFX.Web.Social.SocialNetworkStarter, NFX.Web'
      application-start-break-on-exception=true
    }
  }  

  file-systems
  {
  /*
    file-system 
    { 
      name='NFX-Local'
      type='NFX.IO.FileSystem.Local.LocalFileSystem, NFX'
    }*/

    file-system 
    { 
      name='NFX-SVN'
      type='NFX.IO.FileSystem.SVN.SVNFileSystem, NFX.Web'

      default-session-connect-params
      {
        server-url='[SVN_SERVER_URL]' 
        user-name='[SVN_USER_NAME]' 
        user-password='[SVN_USER_PASSWORD]'
      }
    }

    file-system 
    { 
      name='[S3_FS_NAME]'
      type='NFX.IO.FileSystem.S3.V4.S3V4FileSystem, NFX.Web'

      default-session-connect-params
      {
        bucket='[S3_BUCKET]' 
        region='[S3_REGION]' 
        access-key='[S3_ACCESSKEY]' 
        secret-key='[S3_SECRETKEY]'
      }
    }
  }

  web-settings
  {
    service-point-manager 
    { 
      policy
      {
        default-certificate-validation
        {
          case { uri='[SVN_SERVER_URL]' trusted=true}
          case { uri='[S3_SERVER_URL]' trusted=true}
          case { uri='[STRIPE_SERVER_URL]' trusted=true}
        }
      }
    } 

    payment-processing
    {
      pay-system
      {
        name='Stripe'
        type='NFX.Web.Pay.Stripe.StripeSystem, NFX.Web'
        auto-start=true

        default-session-connect-params
        {
          name='StripePrimary'
          type='NFX.Web.Pay.Stripe.StripeConnectionParameters, NFX.Web'

          secret-key='[STRIPE_SECRET_KEY]'
          email='stripe_user@mail.com'

          pay-system-host
          {
            name='StripePrimary'
            type='NFX.Web.Pay.DefaultPaySystemHost, NFX.Web'
          }
        }
      }

      pay-system
      {
        name='Mock'
        type='NFX.Web.Pay.Mock.MockSystem, NFX.Web'
        auto-start=true

        default-session-connect-params
        {
          type='NFX.Web.Pay.Mock.MockConnectionParameters, NFX.Web'
          email='mock_user@mail.com'

          accounts
          {
            account-actual-data
            { 
              account { identity='system' identity-id='1' account-id='2' }

              account-number='212223'
              routing-number='1111'
            }

            account-actual-data
            { 
              account { identity='user' identity-id='112233' account-id='12345890' }

              first-name='vasya'
              middle-name='s'
              last-name='pupkin'

              account-number='111122223333'
              routing-number='0987'
              card-expiration-year='2016'
              card-expiration-month='10'
              card-vc='123'
            }
          }
        }
      }
    }

    social
    {
      provider {name='GooglePlusTest' type='NFX.Web.Social.GooglePlus, NFX.Web' auto-start=true
                  client-code='[SN_GP_CLIENT_CODE]' client-secret='[SN_GP_CLIENT_SECRET]' 
                  web-service-call-timeout-ms='20000' keep-alive='false' pipelined='false'}

      provider { name='FacebookTest' type='NFX.Web.Social.Facebook, NFX.Web' auto-start=true
                  client-code='[SN_FB_CLIENT_CODE]' client-secret='[SN_FB_CLIENT_SECRET]' app-accesstoken='[SN_FB_APP_ACCESSTOKEN]' }

      provider {name='TwitterTest' type='NFX.Web.Social.Twitter, NFX.Web' auto-start=true 
                  client-code='[SN_TWT_CLIENT_CODE]' client-secret='[SN_TWT_CLIENT_SECRET]'}

      provider {name='LinkedInTest' type='NFX.Web.Social.LinkedIn, NFX.Web' auto-start=true 
                  api-key='[SN_LIN_API_KEY]' secret-key='[SN_LIN_SECRET_KEY]'}

      provider {name='VKontakteTest' type='NFX.Web.Social.VKontakte, NFX.Web' auto-start=true 
                  client-code='[SN_VK_CLIENT_CODE]' client-secret='[SN_VK_CLIENT_SECRET]'} 
    }

    web-dav
    {
      log-type='Log.MessageType.TraceZ'
    }

    session
    {
      timeout-ms='30001'
      web-strategy
      {
        cookie-name='kalabashka'
      }
    }
  }

}
";

    #endregion

    #region SVN

      public const string NFX_SVN = "NFX_SVN";

      protected string SVN_ROOT;
      protected string SVN_UNAME;
      protected string SVN_UPSW;

    #endregion

    #region Social

      protected const string NFX_SOCIAL = "NFX_SOCIAL";

      protected const string NFX_SOCIAL_PROVIDER_GP = "GooglePlusTest";
      protected const string NFX_SOCIAL_PROVIDER_FB = "FacebookTest";
      protected const string NFX_SOCIAL_PROVIDER_TWT = "TwitterTest";
      protected const string NFX_SOCIAL_PROVIDER_LIN = "LinkedInTest";
      protected const string NFX_SOCIAL_PROVIDER_VK = "VKontakteTest";

    #endregion

    #region S3 V4

      protected const string NFX_S3 = "NFX_S3";

      protected string S3_BUCKET;
      protected string S3_REGION;

      protected string S3_ACCESSKEY;
      protected string S3_SECRETKEY;

      protected static string S3_DXW_ROOT;

      protected const string S3_FN1 = "nfxtest01.txt";

      protected const string S3_CONTENTSTR1 = "Amazon S3 is storage for the Internet. It is designed to make web-scale computing easier for developers.";
      protected static byte[] S3_CONTENTBYTES1 = Encoding.UTF8.GetBytes(S3_CONTENTSTR1);
      protected static System.IO.Stream S3_CONTENTSTREAM1 = new System.IO.MemoryStream(Encoding.UTF8.GetBytes(S3_CONTENTSTR1));

    #endregion

    #region Pay

      protected const string NFX_STRIPE = "NFX_STRIPE";

      protected string STRIPE_SECRET_KEY;
      protected string STRIPE_PUBLISHABLE_KEY;

    #endregion

    #region Static

      private ExternalCfg s_Instance;
      private object s_LockObj = new object();

      public ExternalCfg Instance
      {
        get
        {
          if (s_Instance != null) return s_Instance;

          lock(s_LockObj)
          {
            if (s_Instance != null) return s_Instance;
            s_Instance = new ExternalCfg();
          }

          return s_Instance;
        }
      }

    #endregion

    #region ctor

      public ExternalCfg()
      {
        initSocial();
        initSVNConsts();
        initS3V4Consts();
        initStripeConsts();
      }

    #endregion

    #region Inits

      private void initSocial()
      {
        try
        {
          var envVarStr = System.Environment.GetEnvironmentVariable(NFX_SOCIAL);

          var cfg = envVarStr.AsLaconicConfig();

          var providersCfg = cfg.Children.Where(c => c.IsSameName(WebSettings.CONFIG_SOCIAL_PROVIDER_SECTION));


          var gpCfg = providersCfg.Single(p => p.IsSameNameAttr(NFX_SOCIAL_PROVIDER_FB));
          LACONF = LACONF
            .Replace("[SN_GP_CLIENT_CODE]", gpCfg.AttrByName("client-code").Value)
            .Replace("[SN_GP_CLIENT_SECRET]", gpCfg.AttrByName("client-secret").Value);

          var fbCfg = providersCfg.Single(p => p.IsSameNameAttr(NFX_SOCIAL_PROVIDER_FB));
          LACONF = LACONF
            .Replace("[SN_FB_CLIENT_CODE]", fbCfg.AttrByName("client-code").Value)
            .Replace("[SN_FB_CLIENT_SECRET]", fbCfg.AttrByName("client-secret").Value)
            .Replace("[SN_FB_APP_ACCESSTOKEN]", fbCfg.AttrByName("app-accesstoken").Value);

          var twtCfg = providersCfg.Single(p => p.IsSameNameAttr(NFX_SOCIAL_PROVIDER_TWT));
          LACONF = LACONF
            .Replace("[SN_TWT_CLIENT_CODE]", twtCfg.AttrByName("client-code").Value)
            .Replace("[SN_TWT_CLIENT_SECRET]", twtCfg.AttrByName("client-secret").Value);

          var linCfg = providersCfg.Single(p => p.IsSameNameAttr(NFX_SOCIAL_PROVIDER_LIN));
          LACONF = LACONF
            .Replace("[SN_LIN_API_KEY]", linCfg.AttrByName("api-key").Value)
            .Replace("[SN_LIN_SECRET_KEY]", linCfg.AttrByName("secret-key").Value);

          var vkCfg = providersCfg.Single(p => p.IsSameNameAttr(NFX_SOCIAL_PROVIDER_VK));
          LACONF = LACONF
            .Replace("[SN_VK_CLIENT_CODE]", vkCfg.AttrByName("client-code").Value)
            .Replace("[SN_VK_CLIENT_SECRET]", vkCfg.AttrByName("client-secret").Value);

        }
        catch (Exception ex)
        {

          throw new Exception(string.Format(
              "May be environment variable \"{0}\" of format like \"{1}\" isn't present.\nDon't forget to reload VS after variable is added",
                NFX_SOCIAL,
                "social { provider {name='Facebook' type='NFX.Web.Social.Facebook, NFX.Web' client-code='[CLIENT_CODE]' client-secret='[CLIENT_SECRET]' app-accesstoken='[SN_FB_APP_ACCESSTOKEN]'}"),
            ex);
        }
      }

      private void initS3V4Consts()
      {
        try
        {
          string envVarsStr = System.Environment.GetEnvironmentVariable(NFX_S3);

          var cfg = Configuration.ProviderLoadFromString(envVarsStr, Configuration.CONFIG_LACONIC_FORMAT).Root;

          S3_BUCKET = cfg.AttrByName(S3V4FileSystemSessionConnectParams.CONFIG_BUCKET_ATTR).Value;
          S3_REGION = cfg.AttrByName(S3V4FileSystemSessionConnectParams.CONFIG_REGION_ATTR).Value;
          S3_ACCESSKEY = cfg.AttrByName(S3V4FileSystemSessionConnectParams.CONFIG_ACCESSKEY_ATTR).Value;
          S3_SECRETKEY = cfg.AttrByName(S3V4FileSystemSessionConnectParams.CONFIG_SECRETKEY_ATTR).Value;

          var s3ServerUri = NFX.IO.FileSystem.S3.V4.S3V4Sign.S3V4URLHelpers.CreateURI(S3_REGION, S3_BUCKET, string.Empty).AbsoluteUri;

          S3_DXW_ROOT = s3ServerUri + "nfx";

          LACONF = LACONF
            .Replace("[S3_FS_NAME]", NFX_S3)
            .Replace("[S3_BUCKET]", S3_BUCKET)
            .Replace("[S3_REGION]", S3_REGION)
            .Replace("[S3_ACCESSKEY]", S3_ACCESSKEY)
            .Replace("[S3_SECRETKEY]", S3_SECRETKEY)
            .Replace("[S3_SERVER_URL]", s3ServerUri);
        }
        catch (Exception ex)
        {
          throw new Exception(string.Format(
              "May be environment variable \"{0}\" of format like \"{1}\" isn't present.\nDon't forget to reload VS after variable is added",
                NFX_S3,
                "s3{ bucket='bucket01' region='us-west-2' access-key='XXXXXXXXXXXXXXXXXXXX' secret-key='XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX'}"),
            ex);
        }
      }

      private void initSVNConsts()
      {
        try
        {
          string envVarsStr = System.Environment.GetEnvironmentVariable(NFX_SVN);

          var cfg = Configuration.ProviderLoadFromString(envVarsStr, Configuration.CONFIG_LACONIC_FORMAT).Root;

          SVN_ROOT = cfg.AttrByName(SVN_CONN_PARAMS.CONFIG_SERVERURL_ATTR).Value;
          SVN_UNAME = cfg.AttrByName(SVN_CONN_PARAMS.CONFIG_UNAME_ATTR).Value;
          SVN_UPSW = cfg.AttrByName(SVN_CONN_PARAMS.CONFIG_UPWD_ATTR).Value;

          LACONF = LACONF.Replace("[SVN_SERVER_URL]", SVN_ROOT)
            .Replace("[SVN_USER_NAME]", SVN_UNAME)
            .Replace("[SVN_USER_PASSWORD]", SVN_UPSW);
        }
        catch (Exception ex)
        {
          throw new Exception(string.Format(
              "May be environment variable \"{0}\" of format like \"{1}\" isn't present.\nDon't forget to reload VS after variable is added",
                NFX_SVN,
                "svn{ server-url='https://somehost.com/svn/XXX' user-name='XXXX' user-password='XXXXXXXXXXXX' }"),
            ex);
        }
      }

      private void initStripeConsts()
      {
        try
        {
          var stripeEnvStr = System.Environment.GetEnvironmentVariable(NFX_STRIPE);

          var cfg = Configuration.ProviderLoadFromString(stripeEnvStr, Configuration.CONFIG_LACONIC_FORMAT).Root;

          STRIPE_SECRET_KEY = cfg.AttrByName(STRIPE_CONN_PARAMS.CONFIG_SECRETKEY_ATTR).Value;
          STRIPE_PUBLISHABLE_KEY = cfg.AttrByName(STRIPE_CONN_PARAMS.CONFIG_PUBLISHABLEKEY_ATTR).Value;

          LACONF = LACONF.Replace("[STRIPE_SECRET_KEY]", STRIPE_SECRET_KEY)
                          .Replace("[STRIPE_SERVER_URL]", NFX.Web.Pay.Stripe.StripeSystem.BASE_URI);
        }
        catch (Exception ex)
        {
          throw new Exception(string.Format(
              "May be environment variable \"{0}\" of format like \"{1}\" isn't present.\nDon't forget to reload VS after variable is added",
                NFX_STRIPE,
                "stripe{ type='NFX.Web.Pay.Stripe.StripeConnParams' secret-key='sk_xxxx_xXxXXxXXXXXXXxxXXxXXXxxx' publishable-key='pk_xxxx_xXXXxxXXXXxxxXxxxxxxxxXx'}"),
            ex);
        }
      }

    #endregion
  }
}
