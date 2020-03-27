using NextPipe.Core.Domain.SharedValueObjects;

namespace NextPipe.Core.Events.Events.ModuleEvents
{
    public class UninstallModuleEvent : BaseEvent
    {
        public Id ModuleId { get; }
        public Id TaskId { get; }

        public UninstallModuleEvent(Id moduleId, Id taskId = null)
        {
            ModuleId = moduleId;
            TaskId = taskId;
        }
    }
}