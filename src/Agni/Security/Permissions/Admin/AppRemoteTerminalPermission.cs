using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NFX.Security;

namespace Agni.Security.Permissions.Admin
{
    /// <summary>
    /// Controls whether users can access remote terminal of application context
    /// </summary>
    public sealed class AppRemoteTerminalPermission : TypedPermission
    {
       public AppRemoteTerminalPermission() : base(NFX.Security.AccessLevel.VIEW) { }

       public override string Description
       {
           get { return StringConsts.PERMISSION_DESCRIPTION_AppRemoteTerminalPermission; }
       }
    }
}
