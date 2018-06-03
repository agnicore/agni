﻿using System;

using NFX;
using NFX.DataAccess.Distributed;
using NFX.Serialization.BSON;

namespace Agni.Workers
{
  [Serializable]
  public struct SignalFrame
  {
    public const int SERIALIZER_DEFAULT = 0;
    public const int SERIALIZER_BSON = 1;

    public GDID ID;
    public PID PID;

    public Guid Type;
    public DateTime Timestamp;
    public string About;

    public ProcessDescriptor? Descriptor;

    public int Serializer;
    public byte[] Content;

    /// <summary>
    /// Internal. Returns the original instance that was passed to .ctor
    /// This allows to use this structure for dual purpose.
    /// </summary>
    [NonSerialized]
    internal readonly Signal ____CtorOriginal;

    public SignalFrame(Signal signal, int? serializer = SERIALIZER_DEFAULT)
    {
      if (signal == null)
        throw new WorkersException(StringConsts.ARGUMENT_ERROR + "SignalFrame.ctor(signal==null)");

      ____CtorOriginal = signal;

      var qa = GuidTypeAttribute.GetGuidTypeAttribute<Signal, SignalAttribute>(signal.GetType());

      if (serializer.HasValue)
      {
        if (serializer == SERIALIZER_DEFAULT) serializer = SERIALIZER_BSON;
        else
        if (serializer != SERIALIZER_BSON)//we only support BSON for now
          throw new WorkersException(StringConsts.SIGNAL_FRAME_SER_NOT_SUPPORTED_ERROR.Args(signal.GetType().Name, serializer));

        byte[] content;
        try
        {
          var cdoc = RowConverter.DefaultInstance.RowToBSONDocument(signal, null);
          content = cdoc.WriteAsBSONToNewArray();
        }
        catch (Exception error)
        {
          throw new WorkersException(StringConsts.SIGNAL_FRAME_SER_ERROR.Args(signal.GetType().FullName, error.ToMessageWithType()), error);
        }

        this.Serializer = serializer.Value;
        this.Content = content;
      }
      else
      {
        this.Serializer = 0;
        this.Content = null;
      }

      this.ID = signal.SysID;
      this.PID = signal.SysPID;
      this.Type = qa.TypeGuid;
      this.Timestamp = signal.SysCreateTimeStampUTC;
      this.About = signal.SysAbout;
      var resultSignal = signal as ResultSignal;
      this.Descriptor = resultSignal != null ? resultSignal.SysDescriptor : (ProcessDescriptor?)null;
    }

    public Signal Materialize(IGuidTypeResolver resolver)
    {
      if (____CtorOriginal != null) return ____CtorOriginal;

      //1. Resolve type
      var type = resolver.Resolve(this.Type);

      //3. Create signal instance
      var signal = (Signal)NFX.Serialization.SerializationUtils.MakeNewObjectInstance(type);
      signal.____Deserialize(ID, PID, Timestamp, About);
      var resultSignal = signal as ResultSignal;
      if (resultSignal != null)
      {
        if (!Descriptor.HasValue)
          throw new WorkersException("TODO");
        resultSignal.____Deserialize(Descriptor.Value);
      }

      //4. Deserialize content
      if (Serializer != SignalFrame.SERIALIZER_BSON) //for now only support this serializer
        throw new WorkersException(StringConsts.SIGNAL_FRAME_DESER_NOT_SUPPORTED_ERROR.Args(type.Name, Serializer));

      try
      {
        var docContent = BSONDocument.FromArray(Content);
        RowConverter.DefaultInstance.BSONDocumentToRow(docContent, signal, null);
      }
      catch (Exception error)
      {
        throw new WorkersException(StringConsts.SIGNAL_FRAME_DESER_ERROR.Args(type.Name, error.ToMessageWithType()), error);
      }

      return signal;
    }
  }
}
