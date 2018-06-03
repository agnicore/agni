namespace ash
{
  class Program
  {
    static void Main(string[] args)
    {
      new NFX.PAL.NetCore20.NetCore20Runtime();
      Agni.Hosts.ash.ProgramBody.Main(args);
    }
  }
}
