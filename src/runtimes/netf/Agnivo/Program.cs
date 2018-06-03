using System;
using System.Windows.Forms;

using NFX.ApplicationModel;

namespace Agnivo
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
      new NFX.PAL.NetFramework.DotNetFrameworkRuntime();
      using (var application = new ServiceBaseApplication(args, null))
      {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainForm());
      }
    }
  }
}
