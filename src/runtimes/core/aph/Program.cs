namespace aph
{
  class Program
  {
    static void Main(string[] args)
    {
      new NFX.PAL.NetCore20.NetCore20Runtime();
      Agni.Hosts.aph.ProgramBody.Main(args);
    }
  }
}
