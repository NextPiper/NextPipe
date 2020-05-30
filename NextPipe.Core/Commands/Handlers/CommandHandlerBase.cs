using NextPipe.Messaging.Infrastructure.Contracts;

namespace NextPipe.Core.Commands.Handlers
{
    public class CommandHandlerBase
    {
        protected readonly IEventPublisher _eventPublisher;

        public CommandHandlerBase(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }
    }
}