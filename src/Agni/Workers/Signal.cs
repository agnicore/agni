﻿using System;

using NFX;
using NFX.Environment;
using NFX.DataAccess.CRUD;
using NFX.DataAccess.Distributed;

namespace Agni.Workers
{
  [Serializable]
  public abstract class Signal : AmorphousTypedRow
  {
    /// <summary>
    /// Factory method that creates new Signal based on provided PID
    /// </summary>
    public static TSignal MakeNew<TSignal>(PID pid) where TSignal : Signal, new() { return makeDefault(new TSignal(), pid); }

    /// <summary>
    /// Factory method that creates new Signal based on provided Type, PID and Configuration
    /// </summary>
    public static Signal MakeNew(Type type, PID pid, IConfigSectionNode args) { return makeDefault(FactoryUtils.MakeAndConfigure<Signal>(args, type), pid); }

    private static TSignal makeDefault<TSignal>(TSignal signal, PID pid) where TSignal : Signal
    {
      signal.m_SysID = AgniSystem.GDIDProvider.GenerateOneGDID(SysConsts.GDID_NS_WORKER, SysConsts.GDID_NAME_WORKER_SIGNAL);
      signal.m_SysPID = pid;
      signal.m_SysTimestampUTC = App.TimeSource.UTCNow;
      signal.m_SysAbout = "{0}@{1}@{2}".Args(App.CurrentCallUser.Name, App.Name, AgniSystem.HostName);
      return signal;
    }

    protected Signal() { }

    private GDID m_SysID;
    private PID m_SysPID;
    private DateTime m_SysTimestampUTC;
    private string m_SysAbout;

    public void ____Deserialize(GDID id, PID pid, DateTime ts, string about)
    { m_SysID = id; m_SysPID = pid; m_SysTimestampUTC = ts; m_SysAbout = about; }

    /// <summary>
    /// Globally-unique ID of the Signal
    /// </summary>
    public GDID SysID { get { return m_SysID; } }

    /// <summary>
    /// Globally-unique ID of the Process
    /// </summary>
    public PID SysPID { get { return m_SysPID; } }

    /// <summary>
    /// When was created
    /// </summary>
    public DateTime SysCreateTimeStampUTC { get { return m_SysTimestampUTC; } }

    /// <summary>
    /// Who is creator
    /// </summary>
    public string SysAbout { get { return m_SysAbout; } }

    /// <summary>
    /// Type Guid
    /// </summary>
    public Guid SysTypeGuid { get { return GuidTypeAttribute.GetGuidTypeAttribute<Signal, SignalAttribute>(GetType()).TypeGuid; } }

    public override string ToString() { return "{0}('{1}')".Args(GetType().Name, SysPID); }

    public override int GetHashCode() { return m_SysID.GetHashCode(); }

    public override bool Equals(Row other)
    {
      var otherSignal = other as Signal;
      if (otherSignal == null) return false;
      return this.m_SysID == otherSignal.m_SysID;
    }

    public void ValidateAndPrepareForDispatch(string targetName)
    {
      DoPrepareForEnqueuePreValidate(targetName);

      var ve = this.Validate(targetName);
      if (ve != null)
        throw new WorkersException(StringConsts.ARGUMENT_ERROR + "Signal.ValidateAndPrepareForEnqueue(todo).validate: " + ve.ToMessageWithType(), ve);

      DoPrepareForEnqueuePostValidate(targetName);
    }

    public override Exception Validate(string targetName)
    {
      var ve = base.Validate(targetName);
      if (ve != null) return ve;

      if (SysID.IsZero)
        return new CRUDFieldValidationException(this, "SysID", "SysID.IsZero, use MakeNew<>() to make new instances");

      return null;
    }

    protected virtual void DoPrepareForEnqueuePreValidate(string targetName) { }
    protected virtual void DoPrepareForEnqueuePostValidate(string targetName) { }
  }

  [Serializable]
  public abstract class ResultSignal : Signal
  {
    /// <summary>
    /// Factory method that creates new Result Signals assigning them new GDID
    /// </summary>
    public static TSignal MakeNew<TSignal>(Process process) where TSignal : ResultSignal, new()
    {
      var result = Signal.MakeNew<TSignal>(process.SysPID);
      result.m_SysDescriptor = process.SysDescriptor;
      return result;
    }

    private ProcessDescriptor m_SysDescriptor;

    public ProcessDescriptor SysDescriptor { get { return m_SysDescriptor; } }

    public void ____Deserialize(ProcessDescriptor descriptor)
    { m_SysDescriptor = descriptor; }
  }
}
