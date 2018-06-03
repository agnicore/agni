//Generated by Agni.Clients.Tools.AgniGluecCompiler

/* Auto generated by Glue Client Compiler tool (gluec)
on 1/25/2017 11:43:46 PM at SEXTOD by Anton
Do not modify this file by hand if you plan to regenerate this file again by the tool as manual changes will be lost
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using NFX.Glue;
using NFX.Glue.Protocol;


namespace Agni.Clients
{
// This implementation needs @Agni.@Contracts.@ITodoQueueClient, so
// it can be used with ServiceClientHub class

  ///<summary>
  /// Client for glued contract Agni.Contracts.ITodoQueue server.
  /// Each contract method has synchronous and asynchronous versions, the later denoted by 'Async_' prefix.
  /// May inject client-level inspectors here like so:
  ///   client.MsgInspectors.Register( new YOUR_CLIENT_INSPECTOR_TYPE());
  ///</summary>
  public class TodoQueue : ClientEndPoint, @Agni.@Contracts.@ITodoQueueClient
  {

  #region Static Members

     private static TypeSpec s_ts_CONTRACT;
     private static MethodSpec @s_ms_Enqueue_0;

     //static .ctor
     static TodoQueue()
     {
         var t = typeof(@Agni.@Contracts.@ITodoQueue);
         s_ts_CONTRACT = new TypeSpec(t);
         @s_ms_Enqueue_0 = new MethodSpec(t.GetMethod("Enqueue", new Type[]{ typeof(@Agni.@Workers.@TodoFrame[]) }));
     }
  #endregion

  #region .ctor
     public TodoQueue(string node, Binding binding = null) : base(node, binding) { ctor(); }
     public TodoQueue(Node node, Binding binding = null) : base(node, binding) { ctor(); }
     public TodoQueue(IGlue glue, string node, Binding binding = null) : base(glue, node, binding) { ctor(); }
     public TodoQueue(IGlue glue, Node node, Binding binding = null) : base(glue, node, binding) { ctor(); }

     //common instance .ctor body
     private void ctor()
     {

     }

  #endregion

     public override Type Contract
     {
       get { return typeof(@Agni.@Contracts.@ITodoQueue); }
     }



  #region Contract Methods

         ///<summary>
         /// Synchronous invoker for  'Agni.Contracts.ITodoQueue.Enqueue'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning '@System.@Int32' or WrappedExceptionData instance.
         /// ClientCallException is thrown if the call could not be placed in the outgoing queue.
         /// RemoteException is thrown if the server generated exception during method execution.
         ///</summary>
         public @System.@Int32 @Enqueue(@Agni.@Workers.@TodoFrame[]  @todos)
         {
            var call = Async_Enqueue(@todos);
            return call.GetValue<@System.@Int32>();
         }

         ///<summary>
         /// Asynchronous invoker for  'Agni.Contracts.ITodoQueue.Enqueue'.
         /// This is a two-way call per contract specification, meaning - the server sends the result back either
         ///  returning no exception or WrappedExceptionData instance.
         /// CallSlot is returned that can be queried for CallStatus, ResponseMsg and result.
         ///</summary>
         public CallSlot Async_Enqueue(@Agni.@Workers.@TodoFrame[]  @todos)
         {
            var request = new RequestAnyMsg(s_ts_CONTRACT, @s_ms_Enqueue_0, false, RemoteInstance, new object[]{@todos});
            return DispatchCall(request);
         }


  #endregion

  }//class
}//namespace
