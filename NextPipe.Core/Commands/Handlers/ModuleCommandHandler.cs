using System.Threading;
using System.Threading.Tasks;
using NextPipe.Core.Commands.Commands.ModuleCommands;
using NextPipe.Messaging.Infrastructure.Contracts;
using NextPipe.Utilities.Documents.Responses;
using SimpleSoft.Mediator;

namespace NextPipe.Core.Commands.Handlers
{
    public class ModuleCommandHandler : CommandHandlerBase, 
        ICommandHandler<RequestInstallModule, TaskRequestResponse>
    {
        public ModuleCommandHandler(IEventPublisher eventPublisher) : base(eventPublisher)
        {
            
        }

        public Task<TaskRequestResponse> HandleAsync(RequestInstallModule cmd, CancellationToken ct)
        {
            
        }
    }
}