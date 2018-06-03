using NFX;
using NFX.Environment;

using Agni.AppModel.Terminal;
using Agni.Coordination;
using Agni.Workers;

using Agni.Demo.Todos;

namespace Agni.Demo.Cmdlets
{
  public class Launch : Cmdlet
  {
    public const string CONFIG_COUNT_ATTR = "count";
    public const string CONFIG_AFTER_SEC_ATTR = "after-sec";

    public const int DEFAULT_AFTER_SEC = 60;

    public Launch(AppRemoteTerminal terminal, IConfigSectionNode args) : base(terminal, args) { }

    public override string Execute()
    {
      var count = m_Args.AttrByName(CONFIG_COUNT_ATTR).ValueAsInt(LaunchDemoProcessTodo.DEFAULT_PART_COUNT);
      var afterSec = m_Args.AttrByName(CONFIG_AFTER_SEC_ATTR).ValueAsInt(DEFAULT_AFTER_SEC);

      var pid = AgniSystem.ProcessManager.Allocate(AgniSystem.HostMetabaseSection.ParentZone.RegionPath);

      var todo = Todo.MakeNew<LaunchDemoProcessTodo>();
      todo.PID = pid;
      todo.PartCount = count;
      todo.SysStartDate = App.TimeSource.UTCNow.AddSeconds(afterSec);
      AgniSystem.ProcessManager.Enqueue(todo, "todo", "todoqueue");

      return pid.ToString();
    }

    public override string GetHelp()
    {
      return
@"Launch demo process.
           Parameters:
            <f color=yellow>count=int<f color=gray> - work part count
            <f color=yellow>after-sec=int-sec<f color=gray> - launch after sec
";
    }
  }
}
