using System;
using System.Threading;
using System.Threading.Tasks;
using NextPipe.Core.Queries.Queries;
using SimpleSoft.Mediator;

namespace NextPipe.Core.Queries.Handlers
{
    public class TrialQueryHandler : IQueryHandler<TrialQuery, string>
    {
        public async Task<string> HandleAsync(TrialQuery query, CancellationToken ct)
        {
            Console.WriteLine("query is working");
            return "Hello";
        }
    }
}