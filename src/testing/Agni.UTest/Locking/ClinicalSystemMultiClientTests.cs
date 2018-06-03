using System;
using System.Collections.Generic;
using System.Linq;

using NFX.Scripting;

using NFX;
using System.Collections.Concurrent;
using System.Threading;
using Agni.Locking;

namespace Agni.UTest.Locking
{
  [Runnable]
  public class ClinicalSystemMulticlientTests : BaseTestRigWithMetabase
  {

      private class Facility
      {
         public int ID;
         public int[] Patients;
      }


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


      private enum BusinessLock{MDSEnter, MDSReview, ARMonthEnd}

      [Run("threadCount=64   nsCount=1 facCount=25  durationSec=30")]
      [Run("threadCount=128  nsCount=1 facCount=25  durationSec=30")]
      [Run("threadCount=128  nsCount=1 facCount=250 durationSec=30")]
      [Run("threadCount=128  nsCount=4 facCount=10  durationSec=30")]
      [Run("threadCount=128  nsCount=4 facCount=250 durationSec=30")]
      [Run("threadCount=1024 nsCount=4 facCount=250 durationSec=30")]
      public void MDSEnterReview_ARMonthEnd(int threadCount, int nsCount, int facCount, int durationSec)
      {
        var namespaces = new string[nsCount];
        for(var i=0; i<nsCount; i++)
         namespaces[i] = "NS_{0}_{1}_{2}_{3}__{4}".Args(threadCount, nsCount, facCount, durationSec, i);

        var facilities = new Facility[facCount];
        for(var i=0; i<facCount; i++)
         facilities[i] = new Facility{ ID = i * 456, Patients = Enumerable.Range(1, 100).Select( n => n).ToArray()};

        var errors = new ConcurrentBag<Exception>();
        var threads = new List<Thread>();

        var totalLockAttempts = 0;
        var totalLockTaken = 0;
        var totalLocksReleaseAttempts = 0;
        var totalLocksReleased = 0;

        var totalMDSEnter = 0;
        var totalMDSReview = 0;
        var totalARMonthEnd = 0;

        var totalMDSEnterTaken = 0;
        var totalMDSReviewTaken = 0;
        var totalARMonthEndTaken = 0;

        for(var i=0; i<threadCount; i++)
        threads.Add(new Thread( () =>
        {
          var user = new Agni.Locking.Server.LockSessionData( new LockSessionID(null), "user X", 10000);
          var sd = DateTime.Now;

          while((DateTime.Now - sd).TotalSeconds < durationSec)
          try
          {
             var ns = namespaces[ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, namespaces.Length-1)];
             var facility = facilities[ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, facilities.Length-1)];
             var patient = facility.Patients[ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, facility.Patients.Length-1)];

             var chance = ExternalRandomGenerator.Instance.NextScaledRandomInteger(0,10);

             LockTransaction businessOperationLockStart;
             LockTransaction businessOperationLockEnd;


             BusinessLock lockType;

             if (chance<3)  //MDS ENTER
             {
               lockType = BusinessLock.MDSEnter;

               businessOperationLockStart =
                  new LockTransaction("MDS Entry for {0}@'{1}'".Args(patient, facility.ID), ns, 0, 0.0d,
                 LockOp.Assert( LockOp.Not(  LockOp.Exists("Month-End", facility.ID.ToString())  )),
                 LockOp.Assert( LockOp.Not(  LockOp.Exists("MDS-Review", facility.ID.ToString(), patient.ToString())  )),
                 LockOp.Assert( LockOp.SetVar("MDS-Entry", facility.ID.ToString(),  patient.ToString(), allowDuplicates: true) )
               );

               businessOperationLockEnd =
                  new LockTransaction("MDS Entry for {0}@'{1}' is done".Args(patient, facility.ID), ns, 0, 0.0d,
                    LockOp.Assert( LockOp.DeleteVar("MDS-Entry", facility.ID.ToString(), patient.ToString()) )
               );
             }
             else
             if (chance<6)  //MDS REVIEW
             {

               lockType = BusinessLock.MDSReview;

               businessOperationLockStart =
                 new LockTransaction("MDS Review for {0}@'{1}'".Args(patient, facility.ID), ns, 0, 0.0d,
               LockOp.Assert( LockOp.Not(  LockOp.Exists("Month-End", facility.ID.ToString())  )),
               LockOp.Assert( LockOp.Not(  LockOp.Exists("MDS-Entry", facility.ID.ToString(), patient.ToString())  )),
               LockOp.Assert( LockOp.SetVar("MDS-Review", facility.ID.ToString(), patient.ToString(), allowDuplicates: false) )
               );

               businessOperationLockEnd =
                  new LockTransaction("MDS Review for {0}@'{1}' is done".Args(patient, facility.ID), ns, 0, 0.0d,
                    LockOp.Assert( LockOp.DeleteVar("MDS-Review", facility.ID.ToString(), patient.ToString()) )
               );
             }
             else  // MONTH END
             {
               lockType = BusinessLock.ARMonthEnd;

               //note, this is FACILITY-level lock (unlike MDSs that are patient-level)
               businessOperationLockStart =
                  new LockTransaction("Month End Close@'{0}'".Args(facility.ID), ns, 0, 0.0d,
                LockOp.Assert( LockOp.Not(  LockOp.Exists("MDS-Entry", facility.ID.ToString())  )),
                LockOp.Assert( LockOp.Not(  LockOp.Exists("MDS-Review", facility.ID.ToString())  )),
                LockOp.Assert( LockOp.SetVar("Month-End", facility.ID.ToString(), null) )
              );

               businessOperationLockEnd = new LockTransaction("Month End Close@'{0}' is done".Args(facility.ID), ns, 0, 0.0d,
                LockOp.Assert( LockOp.DeleteVar("Month-End", facility.ID.ToString()) )
              );
             }


             Interlocked.Increment(ref totalLockAttempts);
             if (lockType== BusinessLock.MDSEnter) Interlocked.Increment(ref totalMDSEnter);
             else if (lockType== BusinessLock.MDSReview) Interlocked.Increment(ref totalMDSReview);
             else Interlocked.Increment(ref totalARMonthEnd);

             var tranResult = m_Server.ExecuteLockTransaction(user, businessOperationLockStart);
             if (tranResult.Status==LockStatus.TransactionOK)
             {
               Interlocked.Increment(ref totalLockTaken);

               if (lockType== BusinessLock.MDSEnter) Interlocked.Increment(ref totalMDSEnterTaken);
               else if (lockType== BusinessLock.MDSReview) Interlocked.Increment(ref totalMDSReviewTaken);
               else Interlocked.Increment(ref totalARMonthEndTaken);

               Thread.Sleep( 10 + ExternalRandomGenerator.Instance.NextScaledRandomInteger(0, 150));//immitate some business work

               Interlocked.Increment(ref totalLocksReleaseAttempts);
               tranResult = m_Server.ExecuteLockTransaction(user, businessOperationLockEnd);
               if (tranResult.Status==LockStatus.TransactionOK)
               {
                Interlocked.Increment( ref totalLocksReleased);
               }
             }
             else
             {
               //locking conflict
               Aver.IsTrue(LockErrorCause.Statement == tranResult.ErrorCause);
             }




          }
          catch(Exception error)
          {
            errors.Add(error);
            Console.WriteLine("Total trans: {0}; ERROR: {1}".Args(totalLockAttempts, error.ToMessageWithType()));
          }


        }));

        foreach(var thread in threads)
         thread.Start();

        foreach(var thread in threads)
         thread.Join();

        Console.WriteLine(
@"
Lock attempts: {0} 
  taken:   {1}
------------------------------
Release attempts: {2}
  released: {3}".Args(totalLockAttempts, totalLockTaken, totalLocksReleaseAttempts, totalLocksReleased));

        Console.WriteLine(
@"
  MDS Enter:    {0} out of {1}
  MDS Reviews:  {2} out of {3}
  AR Month End: {4} out of {5}
".Args( totalMDSEnterTaken, totalMDSEnter,
        totalMDSReviewTaken, totalMDSReview,
        totalARMonthEndTaken, totalARMonthEnd));

        Aver.AreEqual(0, errors.Count);
      }
  }
}
