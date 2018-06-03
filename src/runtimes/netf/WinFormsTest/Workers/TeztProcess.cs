using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NFX.DataAccess.CRUD;

using Agni.Workers;
using Agni;

namespace WinFormsTest.Workers
{
  [Process("733756E1-AA35-4F46-ACED-640379B317C5", Description = "Tezt Process")]
  public class TeztProcess : Process
  {
    protected override ResultSignal DoAccept(IProcessHost host, Signal signal) { return null; }


    protected internal override void Merge(IProcessHost host, DateTime utcNow, Process another)
    {
    }
  }
}
