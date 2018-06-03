using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX;
using NFX.Security;
using NFX.Serialization.JSON;

namespace Agni.Security
{
  /// <summary>
  /// Represents AuthenticationToken in textual form that can be stored.
  /// SecurityManager implementations in Agni are expected to use string token.Data
  /// </summary>
  public static class AgniAuthenticationTokenSerializer
  {
    public static string Serialize(AuthenticationToken token)
    {
      var data = token.Data;
      if (data != null)
      {
        if (!(data is string))
          throw new SecurityException("AgniAuthenticationTokenSerializer can not serialize unexpected data '{0}'. Token.Data must be of 'string' type".Args(data.GetType().FullName));
      }

      return new { r = token.Realm, d = data }.ToJSON(JSONWritingOptions.CompactASCII);
    }

    public static AuthenticationToken Deserialize(string token)
    {
      try
      {
        var dataMap = JSONReader.DeserializeDataObject(token) as JSONDataMap;
        var realm = dataMap["r"].AsString();
        var data = dataMap["d"].AsString();

        return new AuthenticationToken(realm, data);
      }
      catch (Exception error)
      {
        throw new SecurityException("AgniAuthenticationTokenSerializer can not deserialize unexpected data", error);
      }
    }
  }
}
