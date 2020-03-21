using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NextPipe.Core.Commands.Commands.StartupCommands;
using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Core.Events.Events;
using NextPipe.Core.Kubernetes;
using NextPipe.Messaging.Infrastructure.Contracts;
using NextPipe.Persistence.Entities;
using NextPipe.Persistence.Repositories;
using NextPipe.Utilities.Documents.Responses;
using SimpleSoft.Mediator;
using TaskStatus = NextPipe.Persistence.Entities.TaskStatus;

namespace NextPipe.Core.Commands.Handlers
{
    public class StartupCommandHandler : CommandHandlerBase,
        ICommandHandler<RequestInitializeInfrastructure, TaskRequestResponse>
    {
        private readonly ITasksRepository _tasksRepository;
        private readonly IRabbitDeploymentManager _rabbitDeploymentManager;


        public StartupCommandHandler(IEventPublisher eventPublisher, ITasksRepository tasksRepository, IRabbitDeploymentManager rabbitDeploymentManager) : base(eventPublisher)
        {
            _tasksRepository = tasksRepository;
            _rabbitDeploymentManager = rabbitDeploymentManager;
        }

        public async Task<TaskRequestResponse> HandleAsync(RequestInitializeInfrastructure cmd, CancellationToken ct)
        {
            // Check if there is already a task running
            var result = await _tasksRepository.GetTasksByTaskType(TaskType.RabbitInfrastructureDeploy);
            if (result.Any())
            {
                if (result.FirstOrDefault().QueueStatus != QueueStatus.Completed)
                {
                    // This means that there is a RabbitInfrastructureDeploy type which is either pending or running. Reply with an AttachingToProcess msg
                    return TaskRequestResponse.AttachToRunningProcess(result.FirstOrDefault().TaskId, $"Task already queued with {nameof(QueueStatus)}={result.FirstOrDefault().QueueStatus}, attaching to task");
                }
            }
            
            // No task is provisioning the infrastructure. See if it is already up and running with the desired number of ready replicas
            // we will not wait or loop for the ready replicas to come online here, that should be the responsibility of the respective task and thus also do the cleanup if it failed
            if (_rabbitDeploymentManager.IsInfrastructureRunning(cmd.LowerBoundaryReadyReplicas.Value))
            {
                // Have a look at the Task repository in order to check that there is no running Task of type NextPipeInfrastructure Provisioning
                return TaskRequestResponse.InfrastructureAlreadyRunning("Task was not accepted, the infrastructure is already running");
            }
            
            // The task has not been queued and the infrastructure has not been provisioned, accept infrastructure initialize request
            // and publish event
            var taskId = new Id();
            await _tasksRepository.Insert(new NextPipeTask
            {
                CreatedAt = DateTime.Now,
                EditedAt = DateTime.Now,
                Id = Guid.NewGuid(),
                QueueStatus = QueueStatus.Pending,
                TaskId = taskId.Value,
                TaskStatus = TaskStatus.Ready,
                TaskPriority = TaskPriority.Fatal,
                TaskType = TaskType.RabbitInfrastructureDeploy
            });

            _eventPublisher.PublishAsync(new InitializeInfrastructureTaskRequestEvent(taskId,
                cmd.LowerBoundaryReadyReplicas, cmd.ReplicaFailureThreshold,
                cmd.ReplicaDelaySeconds));

            return TaskRequestResponse.TaskRequestAccepted(taskId.Value, "Infrastructure Initialize Request Accepted");
        }
    }
}