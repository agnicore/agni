namespace ascon
{
  class Program
  {
    static void Main(string[] args)
    {
      new NFX.PAL.NetCore20.NetCore20Runtime();
      Agni.Tools.ascon.ProgramBody.Main(args);
    }
  }
}
