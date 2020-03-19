using System;
using System.Threading;
using System.Threading.Tasks;
using NextPipe.Core.Commands.Commands.StartupCommands;
using NextPipe.Core.Events.Events;
using NextPipe.Core.Kubernetes;
using NextPipe.Messaging.Infrastructure.Contracts;
using NextPipe.Persistence.Repositories;
using NextPipe.Utilities.Documents.Responses;
using SimpleSoft.Mediator;

namespace NextPipe.Core.Commands.Handlers
{
    public class StartupCommandHandler : CommandHandlerBase,
        ICommandHandler<RequestInitializeInfrastructure, Response>
    {
        private readonly ITasksRepository _tasksRepository;
        private readonly IRabbitDeploymentManager _rabbitDeploymentManager;


        public StartupCommandHandler(IEventPublisher eventPublisher, ITasksRepository tasksRepository, IRabbitDeploymentManager rabbitDeploymentManager) : base(eventPublisher)
        {
            _tasksRepository = tasksRepository;
            _rabbitDeploymentManager = rabbitDeploymentManager;
        }

        public async Task<Response> HandleAsync(RequestInitializeInfrastructure cmd, CancellationToken ct)
        {
            // Create the new object right here and do the validation in there!
            
            // Check if the infrastructure is running, if so reply that infrastructure already has been initialized
            if (_rabbitDeploymentManager.IsInfrastructureRunning(cmd.LowerBoundaryReadyReplicas.Value))
            {
                
            }
            
            
            // Check if InitializeInfrastructureRequest has already been accepted and is being processed by a replica
            
            // Accept the requestInitializeInfrastructure --> Generate Process Id and upload status to DB return success
            // InitializeInfrastructure Process request accepted, and a url to check up on progress. The process should handle
            // updating the 
            
            
            /*var rabbitDeploymentManager = new RabbitDeploymentManager(new RabbitDeploymentConfiguration(
                2,
                6,
                30
            ), _kubectlHelper);*/
            return Response.Success();
        }


        
    }
}