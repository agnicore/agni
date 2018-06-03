namespace azgov
{
  class Program
  {
    static void Main(string[] args)
    {
      new NFX.PAL.NetFramework.DotNetFrameworkRuntime();
      Agni.Hosts.azgov.ProgramBody.Main(args);
    }
  }
}
