using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NFX.Scripting;

using NFX;

using Agni.Locking;

namespace Agni.UTest.Locking
{
  [Runnable]
  public class LoadTests : BaseTestRigWithMetabase
  {

      private Agni.Locking.Server.LockServerService m_Server;


      protected override void DoRigSetup()
      {
        m_Server = new Agni.Locking.Server.LockServerService(null);
        m_Server.Start();
      }

      protected override void DoRigTearDown()
      {
        m_Server.Dispose();
      }


      [Run("parallel=false  varCount=10000   tblCount=1")]
      [Run("parallel=false  varCount=10000   tblCount=16")]
      [Run("parallel=false  varCount=100000  tblCount=1")]
      [Run("parallel=false  varCount=100000  tblCount=8")]
      //note: parallel does not work faster because it goes against the same namespace which locks
      [Run("parallel=true  varCount=10000   tblCount=1")]
      [Run("parallel=true  varCount=10000   tblCount=16")]
      [Run("parallel=true  varCount=100000  tblCount=1")]
      [Run("parallel=true  varCount=100000  tblCount=8")]
      public void PutManyVarsSameNamespace(bool parallel, int varCount, int tblCount)
      {

        var ns = "NS_{0}_{1}_{2}".Args(parallel, varCount, tblCount);

        var user1 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "User 1", 10000);
        var user2 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "User 2", 10000);


        var sw = Stopwatch.StartNew();

        Action<int> put = (i) =>
          {
             var statements = new LockOp.StatementOp[tblCount];
             for(var t=0; t<tblCount; t++)
              statements[t] = LockOp.Assert( LockOp.SetVar("t"+t.ToString(), "v"+i.ToString(), i, "Variable number "+i.ToString()) );

             var insert = new LockTransaction("Set var", ns, 0, 0.0d,    statements);
             var result = m_Server.ExecuteLockTransaction(user1, insert);
             Aver.IsTrue(LockStatus.TransactionOK == result.Status);
          };

        if (parallel)
         Parallel.For(0, varCount, put);
        else
         for(var i=0; i<varCount; i++) put(i);

        Console.WriteLine("Put {0} in {1} ms at {2} ops/sec".Args(varCount, sw.ElapsedMilliseconds, varCount / (sw.ElapsedTicks / (double)Stopwatch.Frequency)));

        sw.Restart();

        Action<int> get = (i) =>
          {
             var statements = new LockOp.StatementOp[tblCount];
             for(var t=0; t<tblCount; t++)
              statements[t] = LockOp.Assert( LockOp.Exists("t"+t.ToString(), "v"+i.ToString(), i) );

             var read = new LockTransaction("Get var", ns, 0, 0.0d,    statements);
             var result = m_Server.ExecuteLockTransaction(user2, read);
             Aver.IsTrue(LockStatus.TransactionOK == result.Status);
          };

        if (parallel)
         Parallel.For(0, varCount, get);
        else
         for(var i=0; i<varCount; i++) get(i);

        Console.WriteLine("Read/verified {0} in {1} ms at {2} ops/sec".Args(varCount, sw.ElapsedMilliseconds, varCount / (sw.ElapsedTicks / (double)Stopwatch.Frequency)));
      }


      //note: parallel does not work faster because it goes against the same namespace which locks
      [Run("parallel=false  varCount=100000  tblCount=8")]
      [Run("parallel=true   varCount=100000  tblCount=8")]
      public void PutManyVarsSameNamespaceAndExpireSession(bool parallel, int varCount, int tblCount)
      {
        var ns = "NS2_{0}_{1}_{2}".Args(parallel, varCount, tblCount);

        var user1 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "User 1", 7);//7 seconds
        var user2 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "User 2", 10000);


        var sw = Stopwatch.StartNew();

        Action<int> put = (i) =>
          {
             var statements = new LockOp.StatementOp[tblCount];
             for(var t=0; t<tblCount; t++)
              statements[t] = LockOp.Assert( LockOp.SetVar("t"+t.ToString(), "v"+i.ToString(), i, "Variable number "+i.ToString()) );

             var insert = new LockTransaction("Set var", ns, 0, 0.0d,    statements);
             var result = m_Server.ExecuteLockTransaction(user1, insert);
             Aver.IsTrue(LockStatus.TransactionOK == result.Status);
          };

        if (parallel)
         Parallel.For(0, varCount, put);
        else
         for(var i=0; i<varCount; i++) put(i);

        Console.WriteLine("Put {0} in {1} ms at {2} ops/sec".Args(varCount, sw.ElapsedMilliseconds, varCount / (sw.ElapsedTicks / (double)Stopwatch.Frequency)));


        sw.Restart();

        Action<int> get = (i) =>
          {
             var statements = new LockOp.StatementOp[tblCount];
             for(var t=0; t<tblCount; t++)
              statements[t] = LockOp.Assert( LockOp.Exists("t"+t.ToString(), "v"+i.ToString(), i) );//EXISTS

             var read = new LockTransaction("Get var", ns, 0, 0.0d,    statements);
             var result = m_Server.ExecuteLockTransaction(user2, read);
             Aver.IsTrue(LockStatus.TransactionOK == result.Status);
          };

        if (parallel)
         Parallel.For(0, varCount, get);
        else
         for(var i=0; i<varCount; i++) get(i);

        Console.WriteLine("Read/verified that all exists {0} in {1} ms at {2} ops/sec".Args(varCount, sw.ElapsedMilliseconds, varCount / (sw.ElapsedTicks / (double)Stopwatch.Frequency)));


        Thread.Sleep(15000);//block for sessions to timeout


        sw.Restart();

        Action<int> get2 = (i) =>
          {
             var statements = new LockOp.StatementOp[tblCount];
             for(var t=0; t<tblCount; t++)
              statements[t] = LockOp.Assert( LockOp.Not( LockOp.Exists("t"+t.ToString(), "v"+i.ToString(), i) ));  //NOT EXISTS

             var read = new LockTransaction("Get var", ns, 0, 0.0d,    statements);
             var result = m_Server.ExecuteLockTransaction(user2, read);
             Aver.IsTrue(LockStatus.TransactionOK == result.Status);
          };

        if (parallel)
         Parallel.For(0, varCount, get2);
        else
         for(var i=0; i<varCount; i++) get2(i);

        Console.WriteLine("Read/verified that all timed-out {0} in {1} ms at {2} ops/sec".Args(varCount, sw.ElapsedMilliseconds, varCount / (sw.ElapsedTicks / (double)Stopwatch.Frequency)));
      }



       //note: parallel does not work faster because it goes against the same namespace which locks
      [Run("parallel=false  varCount=100000  tblCount=8")]
      [Run("parallel=true   varCount=100000  tblCount=8")]
      public void PutManyVarsSameNamespaceAndExpireVariable(bool parallel, int varCount, int tblCount)
      {
        var ns = "NS3_{0}_{1}_{2}".Args(parallel, varCount, tblCount);
        var ved = App.TimeSource.UTCNow.AddSeconds(7);

        var user1 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "User 1", 10000);//the session is long, but expiration will be set on each var
        var user2 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "User 2", 10000);


        var sw = Stopwatch.StartNew();

        Action<int> put = (i) =>
          {
             var statements = new LockOp.StatementOp[tblCount];
             for(var t=0; t<tblCount; t++)
              statements[t] = LockOp.Assert( LockOp.SetVar("t"+t.ToString(), "v"+i.ToString(), i, "Variable number "+i.ToString(), expirationTimeUTC: ved) );//EXPIRATION IS SET!!!!

             var insert = new LockTransaction("Set var", ns, 0, 0.0d,    statements);
             var result = m_Server.ExecuteLockTransaction(user1, insert);
             Aver.IsTrue(LockStatus.TransactionOK == result.Status);
          };

        if (parallel)
         Parallel.For(0, varCount, put);
        else
         for(var i=0; i<varCount; i++) put(i);

        Console.WriteLine("Put {0} in {1} ms at {2} ops/sec".Args(varCount, sw.ElapsedMilliseconds, varCount / (sw.ElapsedTicks / (double)Stopwatch.Frequency)));


        sw.Restart();

        Action<int> get = (i) =>
          {
             var statements = new LockOp.StatementOp[tblCount];
             for(var t=0; t<tblCount; t++)
              statements[t] = LockOp.Assert( LockOp.Exists("t"+t.ToString(), "v"+i.ToString(), i) );//EXISTS

             var read = new LockTransaction("Get var", ns, 0, 0.0d,    statements);
             var result = m_Server.ExecuteLockTransaction(user2, read);
             Aver.IsTrue(LockStatus.TransactionOK == result.Status);
          };

        if (parallel)
         Parallel.For(0, varCount, get);
        else
         for(var i=0; i<varCount; i++) get(i);

        Console.WriteLine("Read/verified that all exists {0} in {1} ms at {2} ops/sec".Args(varCount, sw.ElapsedMilliseconds, varCount / (sw.ElapsedTicks / (double)Stopwatch.Frequency)));


        Thread.Sleep(15000);//block for sessions to timeout


        sw.Restart();

        Action<int> get2 = (i) =>
          {
             var statements = new LockOp.StatementOp[tblCount];
             for(var t=0; t<tblCount; t++)
              statements[t] = LockOp.Assert( LockOp.Not( LockOp.Exists("t"+t.ToString(), "v"+i.ToString(), i) ));  //NOT EXISTS

             var read = new LockTransaction("Get var", ns, 0, 0.0d,    statements);
             var result = m_Server.ExecuteLockTransaction(user2, read);
             Aver.IsTrue(LockStatus.TransactionOK == result.Status);
          };

        if (parallel)
         Parallel.For(0, varCount, get2);
        else
         for(var i=0; i<varCount; i++) get2(i);

        Console.WriteLine("Read/verified that all timed-out {0} in {1} ms at {2} ops/sec".Args(varCount, sw.ElapsedMilliseconds, varCount / (sw.ElapsedTicks / (double)Stopwatch.Frequency)));
      }
  }
}
