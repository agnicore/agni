using System;

using NFX;
using NFX.Scripting;

using Agni.Locking;

namespace Agni.UTest.Locking
{
  /// <summary>
  /// In this test we test DIRECTLY SERVER (bypassing lockmanager on the client)
  /// using the same server instance
  /// </summary>
  [Runnable]
  public class LockServerDirectTests : BaseTestRigWithMetabase
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
      public void T1()
      {
        var tran = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert( LockOp.True )
        );

        var session = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var result = m_Server.ExecuteLockTransaction(session, tran);

        Aver.IsTrue(LockStatus.TransactionOK == result.Status);
      }

      [Run]
      public void T2()
      {
        var tran = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert( LockOp.False )   //assert(false) must fail
        );

        var session = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var result = m_Server.ExecuteLockTransaction(session, tran);

        Aver.IsTrue(LockStatus.TransactionError == result.Status);
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);
        Aver.AreEqual("0:/AssertOp/", result.FailedStatement);
      }

      [Run]
      public void T3()
      {
        var tran = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert( LockOp.Not( LockOp.False ) )   //assert(!false) must pass
        );

        var session = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var result = m_Server.ExecuteLockTransaction(session, tran);

        Aver.IsTrue(LockStatus.TransactionOK == result.Status);
      }

      [Run]
      public void T4()
      {
        var tran = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert( LockOp.Not( LockOp.Not( LockOp.False ) ) )   //assert(!!false) must fail
        );

        var session = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var result = m_Server.ExecuteLockTransaction(session, tran);

        Aver.IsTrue(LockStatus.TransactionError == result.Status);
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);
        Aver.AreEqual("0:/AssertOp/", result.FailedStatement);
      }

      [Run]
      public void T5()
      {
        var tran = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert(  LockOp.Or( LockOp.True, LockOp.False  )  )   //assert(true | false) must pass
        );

        var session = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var result = m_Server.ExecuteLockTransaction(session, tran);

        Aver.IsTrue(LockStatus.TransactionOK == result.Status);
      }

      [Run]
      public void T6()
      {
        var tran = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert(  LockOp.And( LockOp.True, LockOp.False  )  )   //assert(true & false) must fail
        );

        var session = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var result = m_Server.ExecuteLockTransaction(session, tran);

        Aver.IsTrue(LockStatus.TransactionError == result.Status);
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);
        Aver.AreEqual("0:/AssertOp/", result.FailedStatement);
      }

      [Run]
      public void T7()
      {
        var tran = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert(  LockOp.Xor( LockOp.True, LockOp.False  )  )   //assert(true ^ false) must pass
        );

        var session = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var result = m_Server.ExecuteLockTransaction(session, tran);

        Aver.IsTrue(LockStatus.TransactionOK == result.Status);
      }

      [Run]
      public void T8()
      {
        var tran = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert(  LockOp.Xor( LockOp.True, LockOp.True  )  )   //assert(true ^ true) must fail
        );

        var session = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var result = m_Server.ExecuteLockTransaction(session, tran);

        Aver.IsTrue(LockStatus.TransactionError == result.Status);
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);
        Aver.AreEqual("0:/AssertOp/", result.FailedStatement);
      }

      [Run]
      public void T9()
      {
        var tran = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert(  LockOp.Xor( LockOp.False, LockOp.False  )  )   //assert(false ^ false) must fail
        );

        var session = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var result = m_Server.ExecuteLockTransaction(session, tran);

        Aver.IsTrue(LockStatus.TransactionError == result.Status);
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);
        Aver.AreEqual("0:/AssertOp/", result.FailedStatement);
      }

      [Run]
      public void T10()
      {
        var tran = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert(  LockOp.True  ),
           LockOp.Assert(  LockOp.And( LockOp.True, LockOp.True)  ),
           LockOp.Assert(  LockOp.Or( LockOp.False, LockOp.True)  ),
           LockOp.Assert(  LockOp.Not(LockOp.Or( LockOp.False, LockOp.False))  ),
           LockOp.Assert(  LockOp.Not( LockOp.False)  ),
           LockOp.Assert(  LockOp.Xor(LockOp.False, LockOp.True)  ),
           LockOp.Assert(  LockOp.Xor(LockOp.True, LockOp.False)  ),
           LockOp.Assert(  LockOp.Not(LockOp.Xor(LockOp.True, LockOp.True))  ),
           LockOp.Assert(  LockOp.Not(LockOp.Xor(LockOp.False, LockOp.False))  )
        );

        var session = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var result = m_Server.ExecuteLockTransaction(session, tran);

        Aver.IsTrue(LockStatus.TransactionOK == result.Status);
      }

      [Run]
      public void T11()
      {
        var tran = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert(  LockOp.True  ),
           LockOp.Assert(  LockOp.And( LockOp.True, LockOp.True)  ),
           LockOp.Assert(  LockOp.Or( LockOp.False, LockOp.True)  ),
           LockOp.Assert(  LockOp.Not(LockOp.Or( LockOp.False, LockOp.False))  ),
           LockOp.Assert(  LockOp.Not( LockOp.False)  ),
           LockOp.Assert(  LockOp.Xor(LockOp.False, LockOp.True)  ),
           LockOp.Assert(  LockOp.Xor(LockOp.True, LockOp.False)  ),
           LockOp.Assert(  LockOp.Not(LockOp.Xor(LockOp.False, LockOp.True))  ),
           LockOp.Assert(  LockOp.Not(LockOp.Xor(LockOp.False, LockOp.False))  )
        );

        var session = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var result = m_Server.ExecuteLockTransaction(session, tran);

        Aver.IsTrue(LockStatus.TransactionError == result.Status);
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);
        Aver.AreEqual("7:/AssertOp/", result.FailedStatement);
      }


      [Run]
      public void T12()
      {
        var tran = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert(  LockOp.SetVar("table1", "Dima",  12345, "Set Dima to 12345 in table1")  )

        );

        var session1 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var session2 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "Another session", 1000);
        var result = m_Server.ExecuteLockTransaction(session1, tran);

        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        var read = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.SelectVarValue("MY-RESULT", "table1", "Dima")
        );

        result = m_Server.ExecuteLockTransaction(session2, read);

        Aver.IsTrue(LockStatus.TransactionOK == result.Status);
        Console.WriteLine(  result["MY-RESULT"].GetType().FullName );


        var svar = (Agni.Locking.Server.Variable)result["MY-RESULT"];

        Aver.AreEqual(12345, svar.Value.AsInt());
        Aver.AreEqual("Set Dima to 12345 in table1", svar.Description);
      }


      [Run]
      public void T13()
      {
        var tran = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert(  LockOp.SetVar("table2", "Dima",  12345, "Set Dima to 12345")  )

        );

        var session1 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var session2 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "Another session", 1000);
        var result = m_Server.ExecuteLockTransaction(session1, tran);

        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        var read = new LockTransaction("Testing 101", "Different namespace", 0, 0.0d,  //DIFFERENT NAMESPACE
           LockOp.SelectVarValue("MY-RESULT", "table2", "Dima")
        );

        result = m_Server.ExecuteLockTransaction(session2, read);

        Aver.IsTrue(LockStatus.TransactionOK == result.Status);
        Aver.IsNull( result["MY-RESULT"] );

      }

      [Run]
      public void T14()
      {
        var tran = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert(  LockOp.SetVar("table3", "Dima",  12345, "Set Dima to 12345")  )

        );

        var session1 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var session2 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "Another session", 1000);
        var result = m_Server.ExecuteLockTransaction(session1, tran);

        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        var read = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.SelectVarValue("MY-RESULT", "different table", "Dima") //DIFFERENT TABLE NAME
        );

        result = m_Server.ExecuteLockTransaction(session2, read);

        Aver.IsTrue(LockStatus.TransactionOK == result.Status);
        Aver.IsNull( result["MY-RESULT"] );

      }

      [Run]
      public void T15()
      {
        var tran = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert(  LockOp.SetVar("tblMany1", "Dima1",  12345, "Set Dima to 12345")  ),
           LockOp.Assert(  LockOp.SetVar("tblMany1", "Dima1",  78901, "Set Dima to 78901")  )

        );

        var session = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var result = m_Server.ExecuteLockTransaction(session, tran);

        Aver.IsTrue(LockStatus.TransactionError == result.Status);
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);
        Aver.AreEqual("1:/AssertOp/", result.FailedStatement);

      }

      [Run]
      public void T16()
      {
        var tran1 = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert(  LockOp.SetVar("tblMany2", "Dima1",  12345, "Set Dima to 12345")  )

        );

        var tran2 = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert(  LockOp.SetVar("tblMany2", "Dima1",  78901, "Set Dima to 78901", allowDuplicates: true)  ) //DUPLICATES ARE ALLOWED NOW

        );

        var session1 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var session2 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var session3 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);

        var result = m_Server.ExecuteLockTransaction(session1, tran1);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        result = m_Server.ExecuteLockTransaction(session2, tran2);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);


        var read = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.SelectVarValue("MY-RESULT", "tblMany2", "Dima1", selectMany: true)
        );

        result = m_Server.ExecuteLockTransaction(session3, read);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        var svar = (Agni.Locking.Server.Variable[])result["MY-RESULT"];

        Aver.AreEqual(2, svar.Length);
        Aver.AreEqual(12345, svar[0].Value.AsInt());
        Aver.AreEqual("Set Dima to 12345", svar[0].Description);

        Aver.AreEqual(78901, svar[1].Value.AsInt());
        Aver.AreEqual("Set Dima to 78901", svar[1].Description);
      }



      [Run]
      public void T17()
      {
        var tran1 = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert(  LockOp.SetVar("tblMany3", "Dima1",  12345, "Set Dima to 12345")  )

        );

        var tran2 = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert(  LockOp.SetVar("tA", "Key1",  null, "This will be rolled back")  ),
           LockOp.Assert(  LockOp.SetVar("tB", "Key2",  null, "This will be rolled back")  ),
           LockOp.Assert(  LockOp.SetVar("tblMany3", "Dima1",  78901, "Set Dima to 78901", allowDuplicates: false)  ) //DUPLICATES ARE DISALLOWED

        );

        var session1 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var session2 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var session3 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);

        var result = m_Server.ExecuteLockTransaction(session1, tran1);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        result = m_Server.ExecuteLockTransaction(session2, tran2);
        Aver.IsTrue(LockStatus.TransactionError == result.Status); //duplicates are not allowed
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);

        var read = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.SelectVarValue("MY-RESULT", "tA", "Key1")
        );

        result = m_Server.ExecuteLockTransaction(session3, read);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        Aver.IsNull( result["MY-RESULT"] ); //because the change rolled back the whole transaction
      }


      [Run]
      public void T18()
      {
        var tran = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert(  LockOp.SetVar("t1", "Data1",  123, "Set Data1 in tbl1 to 123") ),
           LockOp.Assert(  LockOp.SetVar("t2", "Data1",  123, "Set Data1 in tbl2 to 123") )
        );

        var session1 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var session2 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);

        var result = m_Server.ExecuteLockTransaction(session1, tran);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);


        var exists = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert(
                           LockOp.And
                           (
                             LockOp.Exists("t1", "Data1"),
                             LockOp.Exists("t2", "Data1")
                           )
                        ));


        result = m_Server.ExecuteLockTransaction(session1, exists); //ignore this session, since sessio1 is created under FALSE
        Aver.IsTrue(LockStatus.TransactionError == result.Status);
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);

        result = m_Server.ExecuteLockTransaction(session2, exists); //ignore this session, since session2 is NOT the created under TRUE
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);
      }


      [Run]
      public void T19()
      {
        var set1 = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert(  LockOp.SetVar("Patient", "Barkhan1",  null, "Oleg Petrovich Barkharev") )
        );


        var set2 = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert(  LockOp.SetVar("Patient", "buloch1970",  null, "Semen Genadievich Bulochkin") )
        );


        var session1 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var session2 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var session3 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);

        //1st user
        var result = m_Server.ExecuteLockTransaction(session1, set1);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        //2nd user
        result = m_Server.ExecuteLockTransaction(session2, set2);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);


        var exists = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert(
                           LockOp.And
                           (
                             LockOp.Exists("Patient", "Barkhan1"),
                             LockOp.Exists("Patient", "buloch1970")
                           )
                        ));

        //3rd user sees what the first two have set
        result = m_Server.ExecuteLockTransaction(session3, exists);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        var delete = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert(
                           LockOp.DeleteVar("Patient", "Barkhan1", null)
                        ));

        //2nd user wants to delete what the 1st set, CANT
        result = m_Server.ExecuteLockTransaction(session2, delete);
        Aver.IsTrue(LockStatus.TransactionError == result.Status);
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);

        //3rd user wants to delete what the 1st set, CANT
        result = m_Server.ExecuteLockTransaction(session3, delete);
        Aver.IsTrue(LockStatus.TransactionError == result.Status);
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);

        //3rd user still sees the two original records
        result = m_Server.ExecuteLockTransaction(session3, exists);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        //1st user deletes what the 1st set, CAN
        result = m_Server.ExecuteLockTransaction(session1, delete);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        //3rd user does not see the two original records anymore
        result = m_Server.ExecuteLockTransaction(session3, exists);
        Aver.IsTrue(LockStatus.TransactionError == result.Status);
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);

        //3rd does not see the first, but still sees the second
        result = m_Server.ExecuteLockTransaction(session3, new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Assert(
                           LockOp.And
                           (
                             LockOp.Not( LockOp.Exists("Patient", "Barkhan1") ),
                             LockOp.Exists("Patient", "buloch1970")
                           )
                        )));
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);
      }

      [Run]
      public void T20()
      {
        var tran = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.SelectOperatorValue("A", LockOp.True),
           LockOp.SelectOperatorValue("B", LockOp.Not( LockOp.True ))
        );

        var session = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);

        var result = m_Server.ExecuteLockTransaction(session, tran);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);


        Aver.IsTrue(result["A"].AsBool());
        Aver.IsFalse(result["B"].AsBool());
      }

      [Run]
      public void T21()
      {
        var tran = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Block(
             LockOp.SelectOperatorValue("A", LockOp.True),
             LockOp.SelectOperatorValue("B", LockOp.Not( LockOp.True )
           ))
        );

        var session = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);

        var result = m_Server.ExecuteLockTransaction(session, tran);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);


        Aver.IsTrue(result["A"].AsBool());
        Aver.IsFalse(result["B"].AsBool());
      }

      [Run]
      public void T22()
      {
        var tran = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.SelectConstantValue("A", "YES"),
           LockOp.SelectConstantValue("B", 123),
           LockOp.SelectConstantValue("C", true)
        );

        var session = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);

        var result = m_Server.ExecuteLockTransaction(session, tran);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);


        Aver.AreEqual("YES", result["A"].AsString());
        Aver.AreEqual(123, result["B"].AsInt());
        Aver.IsTrue(result["C"].AsBool());
      }

      [Run]
      public void T23()
      {
        var tran = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.If(
             LockOp.True,//condition
             LockOp.SelectConstantValue("A", "THEN"),
             LockOp.SelectConstantValue("A", "ELZE")
           ),

           LockOp.If(
             LockOp.False,//condition
             LockOp.SelectConstantValue("B", "THEN"),
             LockOp.SelectConstantValue("B", "ELZE")
           ),

            LockOp.If(
             LockOp.False,//condition
             LockOp.SelectConstantValue("C", "THEN")
           )
        );

        var session = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);

        var result = m_Server.ExecuteLockTransaction(session, tran);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        Aver.AreEqual("THEN", result["A"].AsString());
        Aver.AreEqual("ELZE", result["B"].AsString());
        Aver.IsNull(result["C"]);
      }


      [Run]
      public void T24()
      {
        var tran = new LockTransaction("Testing 101", "A", 0, 0.0d,
           LockOp.Block(
             LockOp.SelectConstantValue("A", 123),
             LockOp.If(
               LockOp.False,//condition
               LockOp.SelectConstantValue("B", "THEN"),
               LockOp.Abort()
               )//if
           )
        );

        var session = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);

        var result = m_Server.ExecuteLockTransaction(session, tran);
        Aver.IsTrue(LockStatus.TransactionError == result.Status);
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);
        Aver.AreEqual("0:/BlockOp/IfOp/AbortOp/", result.FailedStatement);
      }


      [Run]
      public void T25()
      {
        var set1 = new LockTransaction("Testing 101", "B", 0, 0.0d,
           LockOp.Assert( LockOp.SetVar("Doctor", "Sbaitso", "data{ age=23}", "Description 1",  allowDuplicates: true) )
        );

        var set2 = new LockTransaction("Testing 101", "B", 0, 0.0d,
           LockOp.Assert( LockOp.SetVar("Doctor", "Sbaitso", "data{ score=3489}", "Description 1",  allowDuplicates: true) ),
           LockOp.Assert( LockOp.SetVar("Doctor", "Lector", "zhaba", "Description 3",  allowDuplicates: true) )
        );


        var sessionSetter1 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var sessionSetter2 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var sessionGetter = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);

        var result = m_Server.ExecuteLockTransaction(sessionSetter1, set1);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        result = m_Server.ExecuteLockTransaction(sessionSetter2, set2);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        var get = new LockTransaction("Testing 101", "B", 0, 0.0d,
           LockOp.SelectVarValue("A", "Doctor", "Sbaitso", selectMany: true)
        );

        result = m_Server.ExecuteLockTransaction(sessionGetter, get);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        var v = result["A"] as Agni.Locking.Server.Variable[];
        Aver.IsNotNull( v );
        Aver.AreEqual(2, v.Length );
        Console.WriteLine(v[0].Value);
        Aver.AreEqual(23, v[0].Value.AsLaconicConfig().AttrByName("age").ValueAsInt());
        Aver.AreEqual(3489, v[1].Value.AsLaconicConfig().AttrByName("score").ValueAsInt());

        var del1 = new LockTransaction("Testing 101", "B", 0, 0.0d,
           LockOp.Assert( LockOp.DeleteVar("Doctor", "Sbaitso", "data{ age=12111113}") )
        );

        result = m_Server.ExecuteLockTransaction(sessionSetter1, del1);
        Aver.IsTrue(LockStatus.TransactionError == result.Status); //because DATA does not match
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);


        result = m_Server.ExecuteLockTransaction(sessionGetter, get);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        v = result["A"] as Agni.Locking.Server.Variable[];
        Aver.IsNotNull( v );
        Aver.AreEqual(2, v.Length );
        Console.WriteLine(v[0].Value);
        Aver.AreEqual(23, v[0].Value.AsLaconicConfig().AttrByName("age").ValueAsInt());
        Aver.AreEqual(3489, v[1].Value.AsLaconicConfig().AttrByName("score").ValueAsInt());


        del1 = new LockTransaction("Testing 101", "B", 0, 0.0d,
           LockOp.Assert( LockOp.DeleteVar("Doctor", "Sbaitso", null) )
        );

        //Reexecute delet from setter2, it shpould only remove what he has set keeping setters'1 data
        result = m_Server.ExecuteLockTransaction(sessionSetter2, del1);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        result = m_Server.ExecuteLockTransaction(sessionGetter, get);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        //Setters'1 data is still here, but setter's2 is gone
        v = result["A"] as Agni.Locking.Server.Variable[];
        Aver.IsNotNull( v );
        Aver.AreEqual(1, v.Length );
        Console.WriteLine(v[0].Value);
        Aver.AreEqual(23, v[0].Value.AsLaconicConfig().AttrByName("age").ValueAsInt());
      }



      [Run]
      public void T26()
      {
        var set1 = new LockTransaction("Testing 101", "TestNS", 0, 0.0d,
           LockOp.Assert( LockOp.SetVar("Doctor", "Sbaitso", null, "Description 1") ),
           LockOp.Assert( LockOp.SetVar("Doctor", "Pupkin", null,  "Description 2") )
        );


        var set2_1 = new LockTransaction("Testing 101", "TestNS", 0, 0.0d,
           LockOp.Assert( LockOp.SetVar("Doctor", "Solovei", null, "Description 3") )
        );

        var set2_2 = new LockTransaction("Testing 101", "TestNS", 0, 0.0d,
           LockOp.Assert( LockOp.SetVar("Doctor", "Zhaba", "Doctor Zhaba") ),
           LockOp.Assert( LockOp.SetVar("Doctor", "Voron", "Doctor Voron") ),
           LockOp.Assert( LockOp.DeleteVar("Doctor", "Solovei", null) ), //we Assert that deletion did delete solovei, but the tran will get aborted by the next line
           LockOp.Assert( LockOp.SetVar("Doctor", "Pupkin", "Doctor Pupkin duplicate") )
        );


        var sessionSetter1 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var sessionSetter2 = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);
        var sessionGetter = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);

        var result = m_Server.ExecuteLockTransaction(sessionSetter1, set1);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        result = m_Server.ExecuteLockTransaction(sessionSetter2, set2_1); //2nd session owns SOLOVEI
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        result = m_Server.ExecuteLockTransaction(sessionSetter2, set2_2);
        Aver.IsTrue(LockStatus.TransactionError == result.Status);  //And Zhaba+Voron must be rolled back, Solovei deletion rolled back too
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);
        Aver.AreEqual("3:/AssertOp/", result.FailedStatement); //4th assertion failed

        var get = new LockTransaction("Testing 101", "TestNS", 0, 0.0d,
           LockOp.Assert( LockOp.Exists("Doctor", "Sbaitso") ),
           LockOp.Assert( LockOp.Exists("Doctor", "Pupkin") ),
           LockOp.Assert( LockOp.Exists("Doctor", "Solovei") ),//Solovei still exists, because deleting tran got aborted
           LockOp.Assert( LockOp.Not( LockOp.Exists("Doctor", "Zhaba") )),  //Zhaba did not make it because Pupkin was duplicated by 2nd setter
           LockOp.Assert( LockOp.Not( LockOp.Exists("Doctor", "Voron") ))  //Voron same as Zhaba
        );

        result = m_Server.ExecuteLockTransaction(sessionGetter, get);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);
      }



      [Run]
      public void T27()
      {
        var set = new LockTransaction("Testing 101", "T27", 0, 0.0d,
           LockOp.Assert( LockOp.SetVar("Doctor", "Sbaitso", null, "Description 1") ),
           LockOp.Assert( LockOp.SetVar("Doctor", "Pupkin",  null,  "Description 2") ),
           LockOp.Assert( LockOp.SetVar("Doctor", "Solovei", null, "Description 3") )
        );


        var get3 = new LockTransaction("Testing 101", "T27", 0, 0.0d,
           LockOp.Assert( LockOp.Exists("Doctor", "Sbaitso", ignoreThisSession: false) ),
           LockOp.Assert( LockOp.Exists("Doctor", "Pupkin",  ignoreThisSession: false) ),
           LockOp.Assert( LockOp.Exists("Doctor", "Solovei", ignoreThisSession: false) )
        );

        var deleteOk = new LockTransaction("Testing 101", "T27", 0, 0.0d,
           LockOp.Assert( LockOp.DeleteVar("Doctor", "Solovei") )
        );

        var get2 = new LockTransaction("Testing 101", "T27", 0, 0.0d,
           LockOp.Assert( LockOp.Exists("Doctor", "Sbaitso", ignoreThisSession: false) ),
           LockOp.Assert( LockOp.Exists("Doctor", "Pupkin", ignoreThisSession: false) ),
           LockOp.Assert( LockOp.Not( LockOp.Exists("Doctor", "Solovei", ignoreThisSession: false) ) )
        );

        var deleteFailed = new LockTransaction("Testing 101", "T27", 0, 0.0d,
           LockOp.Assert( LockOp.DeleteVar("Doctor", "Pupkin") ),
           LockOp.Assert( LockOp.False )//this should rollback Pupkin deletion
        );


        var session = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "My session", 1000);

        var result = m_Server.ExecuteLockTransaction(session, set);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        result = m_Server.ExecuteLockTransaction(session, get3); //All 3 doctors in place
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        result = m_Server.ExecuteLockTransaction(session, deleteOk);
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        result = m_Server.ExecuteLockTransaction(session, get2); //Only 2 remaining, 3rd doctor got deleted
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);

        result = m_Server.ExecuteLockTransaction(session, deleteFailed);
        Aver.IsTrue(LockStatus.TransactionError == result.Status);
        Aver.IsTrue(LockErrorCause.Statement == result.ErrorCause);
        Aver.AreEqual("1:/AssertOp/", result.FailedStatement); //2nd assertion failed

        result = m_Server.ExecuteLockTransaction(session, get2); //same as above, because previos deletion of Pupkin failed and got rolled back
        Aver.IsTrue(LockStatus.TransactionOK == result.Status);
      }
  }
}
