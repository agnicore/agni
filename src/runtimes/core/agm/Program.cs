﻿namespace agm
{
  class Program
  {
    static void Main(string[] args)
    {
      new NFX.PAL.NetCore20.NetCore20Runtime();
      Agni.Tools.agm.ProgramBody.Main(args);
    }
  }
}
