using NextPipe.Messaging.Infrastructure.Contracts;

namespace NextPipe.Core.Commands.Handlers
{
    public class ModuleCommandHandler : CommandHandlerBase
    {
        public ModuleCommandHandler(IEventPublisher eventPublisher) : base(eventPublisher)
        {
        }
    }
}