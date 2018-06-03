﻿using NFX;

namespace Agni.UTest
{
  internal static class TestSources
  {
     public static string RPATH
     {
       get
       {
         var vname = Agni.AppModel.BootConfLoader.ENV_VAR_METABASE_FS_ROOT + "_UTEST";
         var result = System.Environment.GetEnvironmentVariable(vname);
         if (result.IsNullOrWhiteSpace())
           throw new AgniException(
@"

  This machine is not setup for unit testing.

  The variable:   `{0}`   must point to test metabase root

".Args(vname));

         return result;
       }
     }


     public static string THIS_HOST
     {
       get
       {
         var vname = Agni.AppModel.BootConfLoader.ENV_VAR_HOST_NAME + "_UTEST";
         var result = System.Environment.GetEnvironmentVariable(vname);
         if (result.IsNullOrWhiteSpace())
           throw new AgniException(
@"

  This machine is not setup for unit testing.

  The variable:   `{0}`   must contain a valid metabase host name

".Args(vname));

         return result;
       }
     }

     public const int PARALLEL_LOOP_TO = 250000;
  }
}
