using NextPipe.Core.Domain.SharedValueObjects;

namespace NextPipe.Core.Events.Events
{
    public class UninstallInfrastructureTaskRequestEvent : BaseEvent
    {
        public Id TaskId { get; }

        public UninstallInfrastructureTaskRequestEvent(Id taskId)
        {
            TaskId = taskId;
        }
    }
}