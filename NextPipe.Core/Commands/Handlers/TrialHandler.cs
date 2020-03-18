using System;
using System.Threading;
using System.Threading.Tasks;
using NextPipe.Core.Commands.Commands;
using NextPipe.Utilities.Documents.Responses;
using SimpleSoft.Mediator;

namespace NextPipe.Core.Commands.Handlers
{
    public class TrialHandler : ICommandHandler<TrialCommand, Response>
    {
        public async Task<Response> HandleAsync(TrialCommand cmd, CancellationToken ct)
        {
            Console.WriteLine("Command reached handler!");
            
            return Response.Success();
        }
    }
}