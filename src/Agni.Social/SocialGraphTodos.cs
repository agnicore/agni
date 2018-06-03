using System.Collections.Generic;
using System.Threading.Tasks;
using Agni.Workers;

namespace Agni.Social
{
  public static class SocialGraphTodos
  {
    public static void EnqueueSubscribtion(Todo todo)                       { AgniSystem.ProcessManager.Enqueue(todo, SocialConsts.HOST_SET_SOCIAL_GRAPH, SocialConsts.SVC_SOCIAL_GRAPH_TODO); }
    public static void EnqueueSubscribtion(IEnumerable<Todo> todos)         { AgniSystem.ProcessManager.Enqueue(todos, SocialConsts.HOST_SET_SOCIAL_GRAPH, SocialConsts.SVC_SOCIAL_GRAPH_TODO); }
    public static Task Async_EnqueueSubscribtion(Todo todo)                 { return AgniSystem.ProcessManager.Async_Enqueue(todo, SocialConsts.HOST_SET_SOCIAL_GRAPH, SocialConsts.SVC_SOCIAL_GRAPH_TODO); }
    public static Task Async_EnqueueSubscribtion(IEnumerable<Todo> todos)   { return AgniSystem.ProcessManager.Async_Enqueue(todos, SocialConsts.HOST_SET_SOCIAL_GRAPH, SocialConsts.SVC_SOCIAL_GRAPH_TODO); }

    public static void EnqueueDelivery(Todo todo)                       { AgniSystem.ProcessManager.Enqueue(todo, SocialConsts.HOST_SET_SUBS_DELIVERY, SocialConsts.SVC_SUBS_DELIBERY_TODO); }
    public static void EnqueueDelivery(IEnumerable<Todo> todos)         { AgniSystem.ProcessManager.Enqueue(todos, SocialConsts.HOST_SET_SUBS_DELIVERY, SocialConsts.SVC_SUBS_DELIBERY_TODO); }
    public static Task Async_EnqueueDelivery(Todo todo)                 { return AgniSystem.ProcessManager.Async_Enqueue(todo, SocialConsts.HOST_SET_SUBS_DELIVERY, SocialConsts.SVC_SUBS_DELIBERY_TODO); }
    public static Task Async_EnqueueDelivery(IEnumerable<Todo> todos)   { return AgniSystem.ProcessManager.Async_Enqueue(todos, SocialConsts.HOST_SET_SUBS_DELIVERY, SocialConsts.SVC_SUBS_DELIBERY_TODO); }

    public static void EnqueueRemove(Todo todo)                       { AgniSystem.ProcessManager.Enqueue(todo, SocialConsts.HOST_SET_SUBS_REMOVE, SocialConsts.SVC_SUBS_REMOVE_TODO); }
    public static void EnqueueRemove(IEnumerable<Todo> todos)         { AgniSystem.ProcessManager.Enqueue(todos, SocialConsts.HOST_SET_SUBS_REMOVE, SocialConsts.SVC_SUBS_REMOVE_TODO); }
    public static Task Async_EnqueueRemove(Todo todo)                 { return AgniSystem.ProcessManager.Async_Enqueue(todo, SocialConsts.HOST_SET_SUBS_REMOVE, SocialConsts.SVC_SUBS_REMOVE_TODO); }
    public static Task Async_EnqueueRemove(IEnumerable<Todo> todos)   { return AgniSystem.ProcessManager.Async_Enqueue(todos, SocialConsts.HOST_SET_SUBS_REMOVE, SocialConsts.SVC_SUBS_REMOVE_TODO); }
  }
}