using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NFX.Scripting;

using Agni.Identification;

using NFX;
using NFX.ApplicationModel;
using NFX.Environment;

namespace Agni.UTest.Identification
{
  [Runnable]
  public class GDIDGen
  {
      const string ROOT_DIR=@"c:\NFX\gdid-utest";

      const string CONFIG =
@"
nfx
{
   log
   {
     destination
     {
       type='NFX.Log.Destinations.ConsoleDestination, NFX'
     }
   }

   gdid-authority
   {
      authority-ids='0,1,2,3,4,5,6,7,8,9,a,b,c,d,e,f'
      persistence
      {
        location{name='Primary' order=0 path=$'"+ROOT_DIR+@"'}
      }
   }


  glue
  {
    bindings
    {
      binding
      {
        name='sync'
        type='NFX.Glue.Native.SyncBinding, NFX'
      }
    }

    servers
    {
      server
      {
        name='IGDIDAuthorityAsync'
        node='sync://*:9999'
        contract-servers='Agni.Identification.GDIDAuthority, Agni'
      }
    }
  }
}
";
      private void prepdirs()
      {
          try{ Directory.Delete(ROOT_DIR, true);  }
          catch(Exception error) { Console.WriteLine(error.ToMessageWithType());}

          System.Threading.Thread.Sleep(2000);

          try{ Directory.CreateDirectory(ROOT_DIR); }
          catch(Exception error) { Console.WriteLine(error.ToMessageWithType());}

          System.Threading.Thread.Sleep(2000);
      }

      [Run]
      public void GD_ParallelGeneration_SameSequence()
      {
          prepdirs();

          var conf  = LaconicConfiguration.CreateFromString(CONFIG);
          using(var app =  new ServiceBaseApplication(null, conf.Root))
           using ( var authority = new GDIDAuthorityService())
           {
              authority.Configure(null);
              authority.Start();
              _parallelGenerationSame();
           }

      }

            private void _parallelGenerationSame()
            {
              const int PASS_CNT = 5000;

              int TOTAL = 0;

              var gen = new GDIDGenerator("AAAA", null);
              gen.AuthorityHosts.Register(new GDIDGenerator.AuthorityHost("sync://127.0.0.1:9999"));

              var lst = new List<ulong>();
              var rnd = new Random();

              var sw = Stopwatch.StartNew();
              Parallel.For(0, PASS_CNT,
                        (_)=>
                        {
                          var BATCH = 32 + rnd.Next(255); //introduces randomness in threads
                          ulong[] arr = new ulong[BATCH];

                          for(int i=0; i<BATCH; i++)
                          {
                           arr[i] = gen.GenerateOneGDID("a", "aseq", 1024).ID;
                          }

                          lock(lst)
                            foreach(var id in arr)
                              lst.Add(id);

                          System.Threading.Interlocked.Add(ref TOTAL, BATCH);
                        });

              var elapsed = sw.ElapsedMilliseconds;


              Aver.AreEqual(lst.Count, lst.Distinct().Count());//all values are distinct

              Console.WriteLine("Processed {0} in {1} ms. at {2}/sec.".Args(TOTAL, elapsed, TOTAL / (elapsed / 1000d)));

            }


      [Run]
      public void GD_ParallelGeneration_DifferentSequences()
      {
          prepdirs();

          var conf  = LaconicConfiguration.CreateFromString(CONFIG);
          using(var app =  new ServiceBaseApplication(null, conf.Root))
           using ( var authority = new GDIDAuthorityService())
           {
              authority.Configure(null);
              authority.Start();
              _parallelGenerationDifferent();
           }

      }


            private void _parallelGenerationDifferent()
            {
              const int PASS_CNT = 5000;

              int TOTAL = 0;

              var gen = new GDIDGenerator("AAAA", null);
              gen.AuthorityHosts.Register(new GDIDGenerator.AuthorityHost("sync://127.0.0.1:9999"));

              var dict = new Dictionary<string, List<ulong>>();
              dict.Add("aseq", new List<ulong>());
              dict.Add("bseq", new List<ulong>());
              dict.Add("cseq", new List<ulong>());
              dict.Add("dseq", new List<ulong>());
              dict.Add("eseq", new List<ulong>());
              dict.Add("fseq", new List<ulong>());
              dict.Add("gseq", new List<ulong>());


              var rnd = new Random();

              var sw = Stopwatch.StartNew();
              Parallel.For(0, PASS_CNT,
                        (n)=>
                        {
                          var seq = dict.Keys.ToList()[n % dict.Keys.Count];

                          var BATCH = 32 + rnd.Next(255); //introduces randomness in threads
                          ulong[] arr = new ulong[BATCH];

                          for(int i=0; i<BATCH; i++)
                          {
                           arr[i] = gen.GenerateOneGDID("a", seq, 1024).ID;
                          }

                          lock(dict[seq])
                            foreach(var id in arr)
                              dict[seq].Add(id);

                          System.Threading.Interlocked.Add(ref TOTAL, BATCH);
                        });

              var elapsed = sw.ElapsedMilliseconds;

              foreach(var kvp in dict)
              {
                Aver.AreEqual(kvp.Value.Count, kvp.Value.Distinct().Count());//all values are distinct
                Console.WriteLine("{0} = {1} ids".Args(kvp.Key, kvp.Value.Count));
              }

              Console.WriteLine("Processed {0} in {1} ms. at {2}/sec.".Args(TOTAL, elapsed, TOTAL / (elapsed / 1000d)));

            }



  }
}
