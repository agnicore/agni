namespace agdida
{
  class Program
  {
    static void Main(string[] args)
    {
      new NFX.PAL.NetFramework.DotNetFrameworkRuntime();
      Agni.Hosts.agdida.ProgramBody.Main(args);
    }
  }
}
