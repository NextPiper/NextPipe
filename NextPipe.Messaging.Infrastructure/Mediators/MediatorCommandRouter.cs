using System.Threading;
using System.Threading.Tasks;
using NextPipe.Messaging.Infrastructure.Contracts;
using SimpleSoft.Mediator;

namespace NextPipe.Messaging.Infrastructure.Mediators
{
    public class MediatorCommandRouter : ICommandRouter
    {
        private readonly IMediator _mediator;

        public MediatorCommandRouter(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        public async Task<TResponse> RouteAsync<TCommand, TResponse>(TCommand command,
            CancellationToken cancellationToken = default(CancellationToken)) where TCommand : ICommand<TResponse>
        {
            return await _mediator.SendAsync<TCommand, TResponse>(command, cancellationToken);
        }
    }
}