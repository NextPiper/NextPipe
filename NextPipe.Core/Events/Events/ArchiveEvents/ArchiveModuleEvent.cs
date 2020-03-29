using NextPipe.Core.Domain.SharedValueObjects;

namespace NextPipe.Core.Events.Events.ArchiveEvents
{
    public class ArchiveModuleEvent : BaseEvent
    {
        public Id ModuleId { get; }

        public ArchiveModuleEvent(Id moduleId)
        {
            ModuleId = moduleId;
        }
    }
}