using System.Reflection;
using NextPipe.Core.Domain.SharedValueObjects;

namespace NextPipe.Core.Events.Events.ModuleEvents
{
    public class InstallModuleEvent : BaseEvent
    {
        public Id ModuleId { get; }
        public Id TaskId { get; }

        public InstallModuleEvent(Id moduleId, Id taskId = null)
        {
            ModuleId = moduleId;
            TaskId = taskId;
        }
    }
}