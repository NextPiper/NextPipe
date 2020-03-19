using System.Threading;
using System.Threading.Tasks;
using NextPipe.Messaging.Infrastructure.Contracts;
using SimpleSoft.Mediator;

namespace NextPipe.Messaging.Infrastructure.Mediators
{
    public class MediatorEventPublisher : IEventPublisher
    {
        private readonly IMediator _mediator;

        public MediatorEventPublisher(IMediator _mediator)
        {
            this._mediator = _mediator;
        }

        public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default(CancellationToken)) where TEvent : IEvent
        {
            await _mediator.BroadcastAsync(@event, ct);
        }
    }
}