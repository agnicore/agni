﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using NFX;

using Agni;
using Agni.Workers;

namespace WinFormsTest.Workers
{
  public partial class ProcessForm : Form
  {
    public ProcessForm()
    {
      InitializeComponent();
    }

    private Agni.Workers.Server.ProcessControllerService m_Server;

    private PID doAllocate(string zonePath, string id, bool isUnique)
    {
      var zone = AgniSystem.Metabase.CatalogReg.NavigateZone(zonePath);
      var processorID = zone.MapShardingKeyToProcessorID(id);

      return new PID(zone.RegionPath, processorID, id.ToString(), isUnique);
    }

    private void btnSpawn_Click(object sender, EventArgs e)
    {
      var pid = AgniSystem.ProcessManager.Allocate("us/east/cle/a/ii");
      var process = Process.MakeNew<TeztProcess>(pid);
      var host = m_Server as IProcessHost;
      host.LocalSpawn(process);
    }

    private void btnServerStart_Click(object sender, EventArgs e)
    {
      var cfg = @"
srv
{
  startup-delay-sec = 1

  process-store
  {
    type='Agni.Workers.Server.Queue.MongoProcessStore, Agni.MongoDB'
    mongo='mongo{server=\'localhost:27017\' db=\'process-tezt\'}'
  }
}".AsLaconicConfig(handling: ConvertErrorHandling.Throw);

      m_Server = new Agni.Workers.Server.ProcessControllerService();
      m_Server.Configure(cfg);
      m_Server.Start();
    }

    private void btnServerStop_Click(object sender, EventArgs e)
    {
      DisposableObject.DisposeAndNull(ref m_Server);
    }

    private void tmr_Tick(object sender, EventArgs e)
    {
      var enabled = m_Server != null;

      btnServerStart.Enabled = !enabled;
      btnServerStop.Enabled = enabled;
      btnSpawn.Enabled = enabled;

      if (enabled)
        lstProcess.DataSource = m_Server.List(0);
    }
  }
}
