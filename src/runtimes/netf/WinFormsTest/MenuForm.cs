﻿using System;
using System.Windows.Forms;
using WinFormsTest.Social;

namespace WinFormsTest
{
  public partial class MenuForm : Form
  {
    public MenuForm()
    {
      InitializeComponent();
    }

    private void btnPay_Click(object sender, EventArgs e)
    {
      new Pay.PayForm().Show();
    }

    private void btnCache_Click(object sender, EventArgs e)
    {
      new Caching.CacheForm().Show();
    }

    private void btnLocking_Click(object sender, EventArgs e)
    {
      new Locker.MDSARLocking().Show();
    }

    private void btnGDID_Click(object sender, EventArgs e)
    {
      new IDGen.GDIDForm().Show();
    }

    private void btnKDB_Click(Object sender,EventArgs e)
    {
      new KDB.KDBForm().Show();
    }

    private void btnTodo_Click(object sender, EventArgs e)
    {
      new Workers.TodoForm().Show();
    }

    private void btnProcess_Click(object sender, EventArgs e)
    {
      new Workers.ProcessForm().Show();
    }

    private void btnTrending_Click(object sender, EventArgs e)
    {
      new TrendingForm().Show();
    }

    private void btnGraphNode_Click(object sender, EventArgs e)
    {
      new TestGraphNodeForm().Show();
    }
  }
}
