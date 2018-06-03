using System;
using System.Windows.Forms;

using Agni;
using Agni.AppModel;

namespace WinFormsTest
{
  static class Program
  {
    [STAThread]
    static void Main()
    {
      new NFX.PAL.NetFramework.DotNetFrameworkRuntime();

      AgniSystem.MetabaseApplicationName = "WinFormsTest";

      try
      {
        run();
      }
      catch (Exception ex)
      {
        MessageBox.Show(ex.Message, "err", MessageBoxButtons.OK, MessageBoxIcon.Error);
      }
    }

    private static void run()
    {
      using (var app = new AgniServiceApplication(SystemApplicationType.TestRig, new string[] {}, null))
      {
        ((Agni.Identification.GDIDGenerator)app.GDIDProvider).TestingAuthorityNode = "sync://127.0.0.1:4000";
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MenuForm());
      }
    }
  }
}
