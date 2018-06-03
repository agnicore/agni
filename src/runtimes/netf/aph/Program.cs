namespace aph
{
  class Program
  {
    static void Main(string[] args)
    {
      new NFX.PAL.NetFramework.DotNetFrameworkRuntime();
      Agni.Hosts.aph.ProgramBody.Main(args);
    }
  }
}
