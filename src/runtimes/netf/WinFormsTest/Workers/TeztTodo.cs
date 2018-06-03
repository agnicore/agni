using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX;
using NFX.DataAccess.CRUD;

using Agni.Workers;

namespace WinFormsTest.Workers
{
  [TodoQueue("tezt", "A0176D65-B43C-4D34-9DBA-EDACB281709F")]
  public class TeztTodo : Todo
  {
    public TeztTodo() { }

    [Field(backendName: "pid")]
    public string PersonID { get; set;}

    [Field(backendName: "pname")]
    public string PersonName { get; set;}

    [Field(backendName: "pdob")]
    public DateTime PersonDOB { get; set;}


    public static int TotalProcessed;

    protected internal override ExecuteState Execute(ITodoHost host, DateTime utcBatchNow)
    {
       System.Threading.Interlocked.Increment(ref TotalProcessed);

       //App.Log.Write( new NFX.Log.Message
       //{
       //  Type = NFX.Log.MessageType.Info,
       //  From = "",
       //  Text = "--------------> {0} {1} {2}".Args(PersonID, PersonName, PersonDOB)
       //});

       return ExecuteState.Complete;
    }
  }

  [TodoQueue("email", "EDA40F42-9B22-4FCC-80C5-BDB99271D4CA")]
  public class EmailXTimesTodo : Todo
  {
    public EmailXTimesTodo() { }

    [Field(backendName: "who")]
    public string Who { get; set;}

    [Field(backendName: "cnt")]
    public int Count { get; set;}

    [Field(backendName: "isec")]
    public int IntervalSec { get; set;}


    protected internal override ExecuteState Execute(ITodoHost host, DateTime utcBatchNow)
    {
       Count--;
       if (Count<0) return ExecuteState.Complete;

       App.Log.Write( new NFX.Log.Message
       {
         Type = NFX.Log.MessageType.Info,
         From = "",
         Text = "--------------> Email nomer #{0} poslan {1}".Args(Count, Who)
       });

       SysStartDate = App.TimeSource.UTCNow.AddSeconds(IntervalSec);

       return ExecuteState.ReexecuteUpdated;
    }
  }


  [TodoQueue("tezt", "AA84E4AE-7B1F-4622-869F-A3D771A16876")]
  public class CorrelatedTeztTodo : CorrelatedTodo
  {
    public CorrelatedTeztTodo() { }

    [Field(backendName: "ctr")]
    public int Counter { get; set;}


    public static int TotalProcessed;

    protected internal override ExecuteState Execute(ITodoHost host, DateTime utcBatchNow)
    {
       System.Threading.Interlocked.Increment(ref TotalProcessed);

 //      System.Threading.Thread.Sleep(NFX.ExternalRandomGenerator.Instance.NextScaledRandomInteger(1000, 3000));

       App.Log.Write( new NFX.Log.Message
       {
         Type = NFX.Log.MessageType.Info,
         From = "",
         Text = "--------------> {0} {1}".Args(SysCorrelationKey, Counter)
       });

        //var t = Todo.NewTodo<CorrelatedTeztTodo>();
        //t.SysCorrelationKey = this.SysCorrelationKey;
        //t.Counter = 1;
        //t.SysStartDate = App.TimeSource.UTCNow.AddSeconds(2);

        //host.Enqueue(t);

       return ExecuteState.Complete;
    }

    protected internal override MergeResult Merge(ITodoHost host, DateTime utcNow, CorrelatedTodo another)
    {
      var at = another as CorrelatedTeztTodo;
      if (at==null) return MergeResult.None;

      //if (at.Counter==73)
      //{
      //   var t = Todo.NewTodo<TeztTodo>();

      //   t.PersonID = "fwdeohfdh";
      //   t.PersonDOB = DateTime.Now;
      //   t.PersonName = "zhabakritskiy";

      //   host.Enqueue( t );
      //}

      //if(at.Counter == 73)
      //{
      //  var t = Todo.NewTodo<CorrelatedTeztTodo>();
      //  t.SysCorrelationKey = this.SysCorrelationKey;
      //  t.Counter = 1;

      //  host.Enqueue(t);
      //}

      if (Counter >= 10) return MergeResult.None;//can not have counter > 10 (business logic)


      Counter += at.Counter;

     // SysStartDate = utcNow.AddSeconds(10);//reschedule for later call
      return MergeResult.Merged;
    }
  }
}
