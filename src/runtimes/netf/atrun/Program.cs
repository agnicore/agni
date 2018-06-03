namespace atrun
{
  class Program
  {
    static void Main(string[] args)
    {
      new NFX.PAL.NetFramework.DotNetFrameworkRuntime();
      NFX.Tools.Trun.ProgramBody.Main(args);
      if (System.Diagnostics.Debugger.IsAttached)
      {
        System.Console.WriteLine("Press <Enter>");
        System.Console.ReadLine();
      }
    }
  }
}
