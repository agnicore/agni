using NFX;
using NFX.Scripting;

using Agni.Locking;

namespace Agni.UTest.Locking
{
  [Runnable]
  public class LockingTranTests : BaseTestRigWithMetabase
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

      //Reflects a case of MDS entry where:
      //Many people can enter MDS for the same patient in the same facility BUT
      //noone can enter if at least one user either reviews MDS for THAT patient, or closes period in the facility.
      //The reverse rules also apply - noone can start revieweing if at least one person enters
      [Run]
      public void MDS_Entry_Review()
      {
        var mdsEnter = new LockTransaction("MDS Entry for Zak Pak@'Cherry Grove'", "Clinical", 0, 0.5d,
               LockOp.Assert( LockOp.Not(  LockOp.Exists("Month-End", "CGROVE-35")  )),
               LockOp.Assert( LockOp.Not(  LockOp.Exists("MDS-Review", "CGROVE-35", "R-45899")  )),
               LockOp.Assert( LockOp.SetVar("MDS-Entry", "CGROVE-35", "R-45899", allowDuplicates: true) )
        );

        var mdsReview = new LockTransaction("MDS Review for Zak Pak@'Cherry Grove'", "Clinical", 0, 0.5d,
               LockOp.Assert( LockOp.Not(  LockOp.Exists("Month-End", "CGROVE-35")  )),
               LockOp.Assert( LockOp.Not(  LockOp.Exists("MDS-Entry", "CGROVE-35", "R-45899")  )),
               LockOp.Assert( LockOp.SetVar("MDS-Review", "CGROVE-35", "R-45899", allowDuplicates: false) )
        );

        var mdsReviewUnlock = new LockTransaction("MDS Review for Zak Pak@'Cherry Grove' is done", "Clinical", 0, 0.5d,
               LockOp.AnywayContinueAfter( LockOp.DeleteVar("MDS-Review", "CGROVE-35", "R-45899") )
        );

        var user1 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "User 1", 1000);
        var user2 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "User 2", 1000);
        var user3 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "User 3", 1000);
        var user4 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "User 4", 1000);

        var result = m_Server.ExecuteLockTransaction(user1, mdsEnter);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        result = m_Server.ExecuteLockTransaction(user2, mdsEnter);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        result = m_Server.ExecuteLockTransaction(user3, mdsReview);
        Aver.IsTrue(LockStatus.TransactionError == result.Status);//Cant start review because someone else is entering
        Aver.AreEqual("1:/AssertOp/", result.FailedStatement);


        //User 1 ends session, this rolls back everything
        Aver.IsTrue( m_Server.EndLockSession( user1.ID ) );

        //still user 2 is present
        result = m_Server.ExecuteLockTransaction(user3, mdsReview);
        Aver.IsTrue(LockStatus.TransactionError == result.Status);//Cant start review because someone else is entering
        Aver.AreEqual("1:/AssertOp/", result.FailedStatement);


        //User 2 ends session, this rolls back everything
        Aver.IsTrue( m_Server.EndLockSession( user2.ID ) );


        //No more entering people REVIEW succeeds
        result = m_Server.ExecuteLockTransaction(user3, mdsReview);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);//Now the review has started!!!!

        //but enter fails because review conflicts with it
        result = m_Server.ExecuteLockTransaction(user4, mdsEnter);
        Aver.IsTrue(LockStatus.TransactionError == result.Status);//Cant start ENTER because someone else is reviewing
        Aver.AreEqual("1:/AssertOp/", result.FailedStatement);

        //Review is done
        result = m_Server.ExecuteLockTransaction(user3, mdsReviewUnlock);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);//Now the review has ended!!!!

        //so enter now succeeds just fine
        result = m_Server.ExecuteLockTransaction(user4, mdsEnter);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);//Can ENTER because review is over
      }



      //Same as above, but adds month end close that must disrupt mDS completely
      [Run]
      public void MDS_Entry_Review_ClosePeriod()
      {
        //note, this is FACILITY-level lock (unlike MDSs that are patient-level)
        var monthCloseStart = new LockTransaction("Month End Close@'Cherry Grove'", "Clinical2", 0, 0.5d,
               LockOp.Assert( LockOp.Not(  LockOp.Exists("MDS-Entry", "CGROVE-35")  )),
               LockOp.Assert( LockOp.Not(  LockOp.Exists("MDS-Review", "CGROVE-35")  )),
               LockOp.Assert( LockOp.SetVar("Month-End", "CGROVE-35", null) )
        );

        var monthCloseUnlock = new LockTransaction("Month End Close@'Cherry Grove' is done", "Clinical2", 0, 0.5d,
               LockOp.AnywayContinueAfter( LockOp.DeleteVar("Month-End", "CGROVE-35") )
        );


        var mdsEnter = new LockTransaction("MDS Entry for Zak Pak@'Cherry Grove'", "Clinical2", 0, 0.5d,
               LockOp.Assert( LockOp.Not(  LockOp.Exists("Month-End", "CGROVE-35")  )),
               LockOp.Assert( LockOp.Not(  LockOp.Exists("MDS-Review", "CGROVE-35", "R-45899")  )),
               LockOp.Assert( LockOp.SetVar("MDS-Entry", "CGROVE-35", "R-45899", allowDuplicates: true) )
        );

        var mdsReview = new LockTransaction("MDS Review for Zak Pak@'Cherry Grove'", "Clinical2", 0, 0.5d,
               LockOp.Assert( LockOp.Not(  LockOp.Exists("Month-End", "CGROVE-35")  )),
               LockOp.Assert( LockOp.Not(  LockOp.Exists("MDS-Entry", "CGROVE-35", "R-45899")  )),
               LockOp.Assert( LockOp.SetVar("MDS-Review", "CGROVE-35", "R-45899", allowDuplicates: true) )
        );

        var mdsReviewUnlock = new LockTransaction("MDS Review for Zak Pak@'Cherry Grove' is done", "Clinical2", 0, 0.5d,
               LockOp.AnywayContinueAfter( LockOp.DeleteVar("MDS-Review", "CGROVE-35", "R-45899") )
        );

        var financialUser1 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "Financial User 1", 1000);
        var financialUser2 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "Financial User 2", 1000);


        var result = m_Server.ExecuteLockTransaction(financialUser1, monthCloseStart);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);//Month end started OK

        result = m_Server.ExecuteLockTransaction(financialUser2, monthCloseStart);
        Aver.IsTrue(LockStatus.TransactionError == result.Status);//Month end start FAILED, as NOTHER user is already locking
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);
        Aver.AreEqual("2:/AssertOp/", result.FailedStatement);


        var user1 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "User 1", 1000);
        var user2 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "User 2", 1000);
        var user3 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "User 3", 1000);
        var user4 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "User 4", 1000);


        result = m_Server.ExecuteLockTransaction(user1, mdsEnter);
        Aver.IsTrue(LockStatus.TransactionError == result.Status); //Cant start MDS entry while closing period
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);
        Aver.AreEqual("0:/AssertOp/", result.FailedStatement);

        result = m_Server.ExecuteLockTransaction(user1, monthCloseUnlock);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status); //mds user can not Delete month close as he did not create it
                                                                //the OK is returned because of 'AnywayContinueAfter' instead of 'Assert'

        result = m_Server.ExecuteLockTransaction(user1, mdsEnter);
        Aver.IsTrue(LockStatus.TransactionError == result.Status); //STILL Cant start MDS entry while closing period
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);
        Aver.AreEqual("0:/AssertOp/", result.FailedStatement);

        result = m_Server.ExecuteLockTransaction(financialUser1, monthCloseUnlock);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);//Now it got deleted


        result = m_Server.ExecuteLockTransaction(user1, mdsEnter);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);//and now , the MDS can start!!!

        result = m_Server.ExecuteLockTransaction(financialUser2, monthCloseStart);
        Aver.IsTrue(LockStatus.TransactionError == result.Status);//Now we cant start month end as someone is entering MDS
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);
        Aver.AreEqual("0:/AssertOp/", result.FailedStatement);


        result = m_Server.ExecuteLockTransaction(user2, mdsEnter);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        result = m_Server.ExecuteLockTransaction(user3, mdsReview);
        Aver.IsTrue(LockStatus.TransactionError == result.Status); //Cant start review because someone else is entering
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);
        Aver.AreEqual("1:/AssertOp/", result.FailedStatement);


        //User 1 ends session, this rolls back everything
        Aver.IsTrue( m_Server.EndLockSession( user1.ID ) );

        //still user 2 is present
        result = m_Server.ExecuteLockTransaction(user3, mdsReview);
        Aver.IsTrue(LockStatus.TransactionError == result.Status); //Cant start review because someone else is entering
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);
        Aver.AreEqual("1:/AssertOp/", result.FailedStatement);


        //User 2 ends session, this rolls back everything
        Aver.IsTrue( m_Server.EndLockSession( user2.ID ) );


        //No more entering people REVIEW succeeds
        result = m_Server.ExecuteLockTransaction(user3, mdsReview);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status); //Now the review has started!!!!


        result = m_Server.ExecuteLockTransaction(financialUser2, monthCloseStart);
        Aver.IsTrue(LockStatus.TransactionError == result.Status); //Now we cant start month end as someone is REVIEWING MDS
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);
        Aver.AreEqual("1:/AssertOp/", result.FailedStatement);

        //but enter fails because review conflicts with it
        result = m_Server.ExecuteLockTransaction(user4, mdsEnter);
        Aver.IsTrue(LockStatus.TransactionError == result.Status); //Cant start ENTER because someone else is reviewing
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);
        Aver.AreEqual("1:/AssertOp/", result.FailedStatement);

        //Review is done
        result = m_Server.ExecuteLockTransaction(user3, mdsReviewUnlock);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status); //Now the review has ended!!!!

        //so enter now succeeds just fine
        result = m_Server.ExecuteLockTransaction(user4, mdsEnter);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status); //Can ENTER because review is over
      }

      [Run]
      public void Bug_20161102_DoubleSetvar()
      {
        var t1 = new LockTransaction("Bug 20161102", "Bug20161102", 0, 0,
          LockOp.Assert(LockOp.And(LockOp.SetVar("TBL_1", "BUG", null, allowDuplicates: false),
                                   LockOp.SetVar("TBL_2", "SAME", null, allowDuplicates: false))));

        var t2 = new LockTransaction("Bug 20161102", "Bug20161102", 0, 0,
          LockOp.Assert(LockOp.And(LockOp.SetVar("TBL_1", "OTHER", null, allowDuplicates: false),
                                   LockOp.SetVar("TBL_2", "SAME", null, allowDuplicates: false))));

        var s1 = new Agni.Locking.Server.LockSessionData(new LockSessionID("host1"), "Session 1", 1000);
        var s2 = new Agni.Locking.Server.LockSessionData(new LockSessionID("host2"), "Session 2", 1000);

        var result = m_Server.ExecuteLockTransaction(s1, t1);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        result = m_Server.ExecuteLockTransaction(s2, t2);
        Aver.IsTrue(LockStatus.TransactionError == result.Status);

        m_Server.EndLockSession(s1.ID);

        result = m_Server.ExecuteLockTransaction(s2, t2);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);
      }
  }
}
