using System;
using System.Threading;
using NFX.Scripting;

using NFX;

using Agni.Locking;

namespace Agni.UTest.Locking
{
  [Runnable]
  public class ExpirationTests : BaseTestRigWithMetabase
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
      public void SessionExpiration()
      {
        var insert = new LockTransaction("Expiration", "NamespaceName", 0, 0.0d,
               LockOp.Assert( LockOp.SetVar("T1", "a", 100) )
        );

        var exists = new LockTransaction("Expiration", "NamespaceName", 0, 0.0d,
               LockOp.Assert( LockOp.Exists("T1", "a") )
        );

        var dummy =  new LockTransaction("Expiration", "NamespaceName", 0, 0.0d,
               LockOp.Assert( LockOp.True )
        );

        var user1 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "User 1", 8); //8 seconds for session max life

        var user2 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "User 2", 10000);


        var result = m_Server.ExecuteLockTransaction(user1, insert);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        result = m_Server.ExecuteLockTransaction(user2, exists);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);//User 2 sees it

        for(var i=0; i<20; i++)
        {
         Thread.Sleep(1000);//Lets wait
         result = m_Server.ExecuteLockTransaction(user1, dummy);
         Aver.IsTrue(LockStatus.TransactionOK == result.Status);//User 1 keeps doing some stuff, so session does not epire
         Console.WriteLine("{0} sec. user 1 keeps sending to session 1.".Args(i));
        }

        result = m_Server.ExecuteLockTransaction(user2, exists);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);//User 2 still sees it because user 1 kept sending requests
        Console.WriteLine(" User 2 still sees session 1 as user 1 kept sending requests");

        for(var i=0; i<20; i++)
        {
         Thread.Sleep(1000);//Lets wait
         result = m_Server.ExecuteLockTransaction(user2, exists);
         if (LockStatus.TransactionError == result.Status && LockErrorCause.Statement == result.ErrorCause) return;//User 2 does not see it as user 1 session expired
         Console.WriteLine("{0} sec. ... has not expired yet...".Args(i));
        }

        Aver.Fail("Did not expire even after significant delay");
      }


      [Run("EXPIRE_IN_SEC=15")]
      [Run("EXPIRE_IN_SEC=30")]
      public void VariableExpiration(int EXPIRE_IN_SEC)
      {
        var expireUTC = App.TimeSource.UTCNow.AddSeconds(EXPIRE_IN_SEC);//EXPIRE_IN_SEC seconds from now it will end

        var insert = new LockTransaction("Expiration", "NamespaceName_"+EXPIRE_IN_SEC, 0, 0.0d,
               LockOp.Assert( LockOp.SetVar("T1", "a", 100, expirationTimeUTC: expireUTC) ),//This will expire soon
               LockOp.Assert( LockOp.SetVar("T1", "b", 200) )                               //but this will only expire with session
        );

        var exists = new LockTransaction("Expiration", "NamespaceName_"+EXPIRE_IN_SEC, 0, 0.0d,
               LockOp.SelectOperatorValue("existsA", LockOp.Exists("T1", "a") ),
               LockOp.SelectOperatorValue("existsB", LockOp.Exists("T1", "b") )
        );


        var user1 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "User 1", 100000);

        var user2 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "User 2", 100000);


        var result = m_Server.ExecuteLockTransaction(user1, insert);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        result = m_Server.ExecuteLockTransaction(user2, exists);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        //User 2 sees it
        Aver.IsTrue(result["existsA"].AsBool());
        Aver.IsTrue(result["existsB"].AsBool());

        for(var i=0; i<EXPIRE_IN_SEC+10; i++)
        {
         Thread.Sleep(1000);//Lets wait
         result = m_Server.ExecuteLockTransaction(user2, exists);
         Aver.IsTrue(LockStatus.TransactionOK == result.Status);

         Aver.IsTrue(result["existsB"].AsBool());
         if (! (bool)result["existsA"] )
         {
           if (i < EXPIRE_IN_SEC-2)
             Aver.Fail("Expired too early");
           return;
         }
         Console.WriteLine("{0} sec. ... has not expired yet...".Args(i));
        }

        Aver.Fail("Did not expire even after significant delay");
      }
  }
}
