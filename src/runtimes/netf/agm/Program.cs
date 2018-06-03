namespace agm
{
  class Program
  {
    static void Main(string[] args)
    {
      new NFX.PAL.NetFramework.DotNetFrameworkRuntime();
      Agni.Tools.agm.ProgramBody.Main(args);
    }
  }
}
