namespace ash
{
  class Program
  {
    static void Main(string[] args)
    {
      new NFX.PAL.NetFramework.DotNetFrameworkRuntime();
      Agni.Hosts.ash.ProgramBody.Main(args);
    }
  }
}
