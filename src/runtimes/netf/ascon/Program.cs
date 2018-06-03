namespace ascon
{
  class Program
  {
    static void Main(string[] args)
    {
      new NFX.PAL.NetFramework.DotNetFrameworkRuntime();
      Agni.Tools.ascon.ProgramBody.Main(args);
    }
  }
}
