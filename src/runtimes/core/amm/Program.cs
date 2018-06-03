namespace amm
{
  class Program
  {
    static void Main(string[] args)
    {
      new NFX.PAL.NetCore20.NetCore20Runtime();
      Agni.Tools.amm.ProgramBody.Main(args);
    }
  }
}
