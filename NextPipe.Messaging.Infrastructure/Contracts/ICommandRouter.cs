using System.Threading;
using System.Threading.Tasks;
using SimpleSoft.Mediator;

namespace NextPipe.Messaging.Infrastructure.Contracts
{
    public interface ICommandRouter
    {
        Task<TResponse> RouteAsync<TCommand, TResponse>(TCommand command, CancellationToken cancellationToken = default(CancellationToken))
            where TCommand : ICommand<TResponse>;
    }
}