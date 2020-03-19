using System.Threading;
using System.Threading.Tasks;
using SimpleSoft.Mediator;

namespace NextPipe.Messaging.Infrastructure.Contracts
{
    public interface IEventPublisher
    {
        Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default(CancellationToken))
            where TEvent : IEvent;
    }
}