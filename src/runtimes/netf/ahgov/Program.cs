namespace ahgov
{
  class Program
  {
    static void Main(string[] args)
    {
      new NFX.PAL.NetFramework.DotNetFrameworkRuntime();
      Agni.Hosts.ahgov.ProgramBody.Main(args);
    }
  }
}
