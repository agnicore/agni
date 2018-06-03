namespace amm
{
  class Program
  {
    static void Main(string[] args)
    {
      new NFX.PAL.NetFramework.DotNetFrameworkRuntime();
      Agni.Tools.amm.ProgramBody.Main(args);
    }
  }
}
