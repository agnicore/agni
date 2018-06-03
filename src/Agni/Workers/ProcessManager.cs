﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

using Agni.AppModel;
using Agni.Coordination;
using NFX.Environment;

namespace Agni.Workers
{
  public sealed class ProcessManager : ProcessManagerBase
  {
    public ProcessManager(IAgniApplication director) : base(director)
    {
    }

    protected override PID DoAllocate(string zonePath, string id, bool isUnique)
    {
      var zone = AgniSystem.Metabase.CatalogReg.NavigateZone(zonePath);
      var processorID = zone.MapShardingKeyToProcessorID(id);

      return new PID(zone.RegionPath, processorID, id, isUnique);
    }

    protected override void DoSpawn<TProcess>(TProcess process)
    {
      var pid = process.SysPID;
      var zone = AgniSystem.Metabase.CatalogReg.NavigateZone(pid.Zone);
      var hosts = zone.GetProcessorHostsByID(pid.ProcessorID);

      Contracts.ServiceClientHub.CallWithRetry<Contracts.IProcessControllerClient>
      (
        (controller) => controller.Spawn(new ProcessFrame(process)),
        hosts.Select(h => h.RegionPath)
      );
    }

    protected override Task Async_DoSpawn<TProcess>(TProcess process)
    {
      var pid = process.SysPID;
      var zone = AgniSystem.Metabase.CatalogReg.NavigateZone(pid.Zone);
      var hosts = zone.GetProcessorHostsByID(pid.ProcessorID);

      return Contracts.ServiceClientHub.CallWithRetryAsync<Contracts.IProcessControllerClient>
      (
        (controller) => controller.Async_Spawn(new ProcessFrame(process)).AsTaskReturningVoid(),
        hosts.Select(h => h.RegionPath)
      );
    }

    protected override ResultSignal DoDispatch<TSignal>(TSignal signal)
    {
      var pid = signal.SysPID;
      var zone = AgniSystem.Metabase.CatalogReg.NavigateZone(pid.Zone);
      var hosts = zone.GetProcessorHostsByID(pid.ProcessorID);

      return Contracts.ServiceClientHub.CallWithRetry<Contracts.IProcessControllerClient, SignalFrame>
      (
        (controller) => controller.Dispatch(new SignalFrame(signal)),
        hosts.Select(h => h.RegionPath)
      ).Materialize(SignalTypeResolver) as ResultSignal;
    }

    protected override Task<ResultSignal> Async_DoDispatch<TSignal>(TSignal signal)
    {
      var pid = signal.SysPID;
      var zone = AgniSystem.Metabase.CatalogReg.NavigateZone(pid.Zone);
      var hosts = zone.GetProcessorHostsByID(pid.ProcessorID);

      return Contracts.ServiceClientHub.CallWithRetryAsync<Contracts.IProcessControllerClient, SignalFrame>
      (
        (controller) => controller.Async_Dispatch(new SignalFrame(signal)).AsTaskReturning<SignalFrame>(),
        hosts.Select(h => h.RegionPath)
      ).ContinueWith((antecedent) => antecedent.Result.Materialize(SignalTypeResolver) as ResultSignal);
    }

    protected override int DoEnqueue<TTodo>(IEnumerable<TTodo> todos, HostSet hs, string svcName)
    {
      var hostPair = hs.AssignHost(todos.First().SysShardingKey);
      return Contracts.ServiceClientHub.CallWithRetry<Contracts.ITodoQueueClient, int>
      (
        (client) => client.Enqueue(todos.Select(t => new TodoFrame(t)).ToArray()),
        hostPair.Select(host => host.RegionPath),
        svcName: svcName
      );
    }

    protected override Task<int> Async_DoEnqueue<TTodo>(IEnumerable<TTodo> todos, HostSet hs, string svcName)
    {
      var hostPair = hs.AssignHost(todos.First().SysShardingKey);
      return Contracts.ServiceClientHub.CallWithRetryAsync<Contracts.ITodoQueueClient, int>
      (
        (client) => client.Async_Enqueue(todos.Select(t => new TodoFrame(t)).ToArray()).AsTaskReturning<int>(),
        hostPair.Select(host => host.RegionPath),
        svcName: svcName
      );
    }

    protected override TProcess DoGet<TProcess>(PID pid)
    {
      var zone = AgniSystem.Metabase.CatalogReg.NavigateZone(pid.Zone);
      var hosts = zone.GetProcessorHostsByID(pid.ProcessorID);

      var processFrame = Contracts.ServiceClientHub.CallWithRetry<Contracts.IProcessControllerClient, ProcessFrame>
      (
        (controller) => controller.Get(pid),
        hosts.Select(h => h.RegionPath)
      );

      // TODO Check type
      return processFrame.Materialize(ProcessTypeResolver) as TProcess;
    }

    protected override ProcessDescriptor DoGetDescriptor(PID pid)
    {
      var zone = AgniSystem.Metabase.CatalogReg.NavigateZone(pid.Zone);
      var hosts = zone.GetProcessorHostsByID(pid.ProcessorID);

      return Contracts.ServiceClientHub.CallWithRetry<Contracts.IProcessControllerClient, ProcessDescriptor>
      (
        (controller) => controller.GetDescriptor(pid),
        hosts.Select(h => h.RegionPath)
      );
    }

    protected override IEnumerable<ProcessDescriptor> DoList(string zonePath, IConfigSectionNode filter)
    {
      var tasks = new List<Task<IEnumerable<ProcessDescriptor>>>();

      var zone = AgniSystem.Metabase.CatalogReg.NavigateZone(zonePath);
      foreach (var processorID in zone.ProcessorMap.Keys)
      {
        var hosts = zone.GetProcessorHostsByID(processorID);
        var descriptors = Contracts.ServiceClientHub.CallWithRetry<Contracts.IProcessControllerClient, IEnumerable<ProcessDescriptor>>
        (
          (controller) => controller.List(processorID),
          hosts.Select(h => h.RegionPath)
        );

        foreach (var descriptor in descriptors)
          yield return descriptor;
      }
    }
  }
}
