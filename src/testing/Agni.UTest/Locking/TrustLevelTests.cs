using System;
using System.Threading;
using NFX.Scripting;

using Agni.Locking;

using NFX;

namespace Agni.UTest.Locking
{
  [Runnable]
  public class TrustLevelTests : BaseTestRigWithMetabase
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

      [Run]
      public void T_000()
      {
        Aver.AreEqual( 0, m_Server.CurrentServerCallsNorm);
        Aver.IsTrue( 0.99d < m_Server.CurrentTrustLevel );
      }

      [Run]
      public void T_001()
      {
        Aver.AreEqual( 0, m_Server.CurrentServerCallsNorm);
        Aver.IsTrue( 0.99d < m_Server.CurrentTrustLevel );


        var tran1 = new LockTransaction("Dummy call to cause traffic", "NamespaceName", 0, 0.75d,
               LockOp.Assert( LockOp.Not(  LockOp.Exists("AAAAA", "BBBBB")  ))
        );

        var user1 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "User 1", 1000);

        //make many calls at once
        LockTransactionResult result;
        for(var i=0; i<16; i++)
        {
          result = m_Server.ExecuteLockTransaction(user1, tran1);
          Aver.IsTrue(LockStatus.TransactionOK == result.Status);
        }

        //then stop
        //Trust level normalizes with time
        while(m_Server.CurrentTrustLevel>0.75d)
        {
          Console.WriteLine(" Waiting for trust to fall  ... Norm: {0}; Trust: {1}".Args(m_Server.CurrentServerCallsNorm, m_Server.CurrentTrustLevel));
          Thread.Sleep(500);
        }

        //trust level is low, failure
        result = m_Server.ExecuteLockTransaction(user1, tran1);
        Aver.IsTrue(LockStatus.TransactionError == result.Status);
        Aver.IsTrue(LockErrorCause.MinimumRequirements == result.ErrorCause);

        //Trust level normalizes with time
        while(m_Server.CurrentTrustLevel<0.75d)
        {
          Console.WriteLine("  ... Norm: {0}; Trust: {1}".Args(m_Server.CurrentServerCallsNorm, m_Server.CurrentTrustLevel));
          Thread.Sleep(500);
        }

        //everything is OK again
        result = m_Server.ExecuteLockTransaction(user1, tran1);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        //for(var i=0; i<120; i++)
        //{
        //  if (i<30 || (i%3)==0)
        //  {
        //  var result = m_Server.ExecuteLockTransaction(user1, tran1);
        //  Aver.AreEqual(LockStatus.TransactionOK, result.Status);
        //  }
        //  if (i==30)
        //    Console.WriteLine("All CALLs stopped----------------");
        //  Thread.Sleep(500);
        //  Console.WriteLine("Made {0} calls; Norm: {1}; Trust: {2}".Args(i, m_Server.CurrentServerCallsNorm, m_Server.CurrentTrustLevel));
        //}
      }
  }
}
