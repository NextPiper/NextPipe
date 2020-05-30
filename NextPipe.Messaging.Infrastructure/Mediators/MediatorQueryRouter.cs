using System.Threading;
using System.Threading.Tasks;
using NextPipe.Messaging.Infrastructure.Contracts;
using SimpleSoft.Mediator;

namespace NextPipe.Messaging.Infrastructure.Mediators
{
    public class MediatorQueryRouter : IQueryRouter
    {
        private readonly IMediator _mediator;

        public MediatorQueryRouter(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken ct = default(CancellationToken)) where TQuery : IQuery<TResult>
        {
            return await _mediator.FetchAsync<TQuery, TResult>(query, ct);
        }
    }
}