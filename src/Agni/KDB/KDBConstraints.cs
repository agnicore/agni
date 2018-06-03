using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX;

namespace Agni.KDB
{
  public static class KDBConstraints
  {
    public const int MAX_TABLE_NAME_LEN = 32;
    public const int MAX_KEY_LEN = 255;

    public static void CheckTableName(string table, string opName)
    {
      if (table.IsNullOrWhiteSpace()) throw new KDBException(StringConsts.KDB_TABLE_IS_NULL_OR_EMPTY_ERROR.Args(opName));

      var len = table.Length;
      if (len > MAX_TABLE_NAME_LEN) throw new KDBException(StringConsts.KDB_TABLE_MAX_LEN_ERROR.Args(opName, len, MAX_TABLE_NAME_LEN));
      for(var i = 0; i < len; i++)
      {
        var c = table[i];
        if ((c >= 'a' && c <= 'z') ||
            (c >= 'A' && c <= 'Z') ||
            (i > 0 && c >= '0' && c <= '9') ||
            c == '_') continue;

        throw new KDBException(StringConsts.KDB_TABLE_CHARACTER_ERROR.Args(opName, table));
      }
    }

    public static void CheckKey(byte[] key, string opName)
    {
      if (key == null || key.Length == 0) throw new KDBException(StringConsts.KDB_KEY_IS_NULL_OR_EMPTY_ERROR.Args(opName));

      var len = key.Length;
      if (len > MAX_KEY_LEN) throw new KDBException(StringConsts.KDB_KEY_MAX_LEN_ERROR.Args(opName, len, MAX_KEY_LEN));
    }
  }
}
