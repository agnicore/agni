﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Reflection;

using NFX;
using NFX.Glue;
using NFX.Glue.Native;
using NFX.Glue.Protocol;
using NFX.Serialization;
using NFX.Serialization.JSON;

namespace Agni.Glue
{
  /// <summary>
  /// Implements server transport for application terminal contract.
  /// READ THIS: this binding processes a few messages a second at best, no need to implement complex optimizations
  /// like copy-free code etc.
  /// </summary>
  public sealed class AppTermServerTransport : SyncServerTransport
  {
     public AppTermServerTransport(SyncBinding binding,
                                   ServerEndPoint serverEndpoint,
                                   IPAddress localAddr, int port) : base(binding, serverEndpoint, localAddr, port)
     {

     }


    protected override RequestMsg DoDecodeRequest(WireFrame frame, MemoryStream ms, ISerializer serializer)
    {
      var utf8 = ms.GetBuffer();
      var json = Encoding.UTF8.GetString(utf8, (int)ms.Position, (int)ms.Length - (int)ms.Position);
      var data = JSONReader.DeserializeDataObject(json) as JSONDataMap;

      if (data==null)
        throw new ProtocolException(StringConsts.GLUE_BINDING_REQUEST_ERROR.Args(nameof(AppTermBinding),"data==null"));

      var reqID = new FID( data["request-id"].AsULong(handling: ConvertErrorHandling.Throw) ); //kuda ego vstavit?
      var instance = data["instance"].AsNullableGUID(handling: ConvertErrorHandling.Throw);
      var oneWay = data["one-way"].AsBool();
      var method = data["method"].AsString();

      MethodSpec mspec;
      if      (method.EqualsOrdIgnoreCase(nameof(Contracts.IRemoteTerminal.Connect)))    mspec = AppTermBinding.METHOD_CONNECT;
      else if (method.EqualsOrdIgnoreCase(nameof(Contracts.IRemoteTerminal.Execute)))    mspec = AppTermBinding.METHOD_EXECUTE;
      else if (method.EqualsOrdIgnoreCase(nameof(Contracts.IRemoteTerminal.Disconnect))) mspec = AppTermBinding.METHOD_DISCONNECT;
      else
       throw new ProtocolException(StringConsts.GLUE_BINDING_REQUEST_ERROR.Args(nameof(AppTermBinding),"unknown method `{0}`".Args(method)));

      var args = data["command"]==null ? new object[0] : new object[]{ data["command"].AsString() };

      var result = new RequestAnyMsg(AppTermBinding.TYPE_CONTRACT, mspec, oneWay, instance, args);

      var autht = data["auth-token"].AsString();
      if (autht!=null)
      {
        var hdr = new AuthenticationHeader(Security.AgniAuthenticationTokenSerializer.Deserialize(autht));
        result.Headers.Add(hdr);
      }
      var authc = data["auth-cred"].AsString();
      if (authc!=null)
      {
        var hdr = new AuthenticationHeader(NFX.Security.IDPasswordCredentials.FromBasicAuth(authc));
        result.Headers.Add(hdr);
      }

      return result;
    }

    protected override void DoEncodeResponse(MemoryStream ms, ResponseMsg msg, ISerializer serializer)
    {
      var data = new JSONDataMap();
      data["id"] = msg.RequestID.ID;
      data["instance"] = msg.RemoteInstance?.ToString("D");

      if (msg.ReturnValue is string rstr)
        data["return"] = rstr;
      else if (msg.ReturnValue is Contracts.RemoteTerminalInfo rtrm)
        data["return"] = rtrm;
      else if (msg.ReturnValue is WrappedExceptionData wed)
        data["return"] = new JSONDataMap{ {"error-content", wed.ToBase64()}};
      else
        throw new ProtocolException(StringConsts.GLUE_BINDING_RESPONSE_ERROR.Args(nameof(AppTermBinding),"unsupported ReturnValue `{0}`".Args(msg.ReturnValue)));

      var json = data.ToJSON(JSONWritingOptions.Compact);
      var utf8 = Encoding.UTF8.GetBytes(json);
      ms.Write(utf8, 0, utf8.Length);
    }
  }
}
