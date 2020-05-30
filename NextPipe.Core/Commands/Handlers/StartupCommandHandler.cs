using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NextPipe.Core.Commands.Commands.StartupCommands;
using NextPipe.Core.Domain.NextPipeTask.ValueObject;
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
        ICommandHandler<RequestInitializeInfrastructure, TaskRequestResponse>,
        ICommandHandler<RequestUninstallInfrastructure, TaskRequestResponse>
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
            // Check if there is already a infrastructureInstal task running
            var nextPipeTask = await IsInfrastructureInstallRequestRunning();
            if (nextPipeTask != null)
            {
                return TaskRequestResponse.AttachToRunningProcess(nextPipeTask.TaskId,
                    $"Task already queued with {nameof(QueueStatus)}={nextPipeTask.QueueStatus}, attaching to task");
            }
            
            // Check if there is already a infrastructuree uninstall event running
            var nextPipeUninstall = await IsInfrastructureUninstallRequestRunning();
            if (nextPipeUninstall != null)
            {
                return TaskRequestResponse.UninstallRunning(nextPipeUninstall.TaskId,
                    "The infrastructure is currently being uninstalled, can't install until it finishes, attaching to uninstall task");
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
                Id = new Id().Value,
                QueueStatus = QueueStatus.Pending,
                TaskId = taskId.Value,
                TaskStatus = TaskStatus.Ready,
                TaskPriority = TaskPriority.Fatal,
                TaskType = TaskType.RabbitInfrastructureDeploy,
                Hostname = new Hostname().Value,
                Logs = "",
                ReferenceId = Guid.Empty
            });

            _eventPublisher.PublishAsync(new InitializeInfrastructureTaskRequestEvent(taskId,
                cmd.LowerBoundaryReadyReplicas, cmd.ReplicaFailureThreshold,
                cmd.ReplicaDelaySeconds, cmd.RabbitNumberOfReplicas));

            return TaskRequestResponse.TaskRequestAccepted(taskId.Value, "Infrastructure Initialize Request Accepted");
        }

        public async Task<TaskRequestResponse> HandleAsync(RequestUninstallInfrastructure cmd, CancellationToken ct)
        {
            // Check if there is already a task running
            var nextPipeTask = await IsInfrastructureUninstallRequestRunning();
            if (nextPipeTask != null)
            {
                return TaskRequestResponse.AttachToRunningProcess(nextPipeTask.TaskId,
                    $"Task already queued with {nameof(QueueStatus)}={nextPipeTask.QueueStatus}, attaching to task");
            }

            var nextPipeInstall = await IsInfrastructureInstallRequestRunning();
            if (nextPipeInstall != null)
            {
                return TaskRequestResponse.InstallRunning(nextPipeInstall.TaskId,
                    $"The infrastructure is currently being installed, can't uninstall until it finishes, attaching to task");
            }
            
            // The task is not running. Queue a task to cleanup the infrastructure
            var taskId = new Id();
            await _tasksRepository.Insert(new NextPipeTask
            {
                CreatedAt = DateTime.Now,
                EditedAt = DateTime.Now,
                Id = new Id().Value,
                QueueStatus = QueueStatus.Pending,
                TaskId = taskId.Value,
                TaskStatus = TaskStatus.Ready,
                TaskPriority = TaskPriority.Fatal,
                TaskType = TaskType.RabbitInfrastructureUninstall,
                Logs = "",
                Hostname = new Hostname().Value,
                ReferenceId = Guid.Empty
            });

            _eventPublisher.PublishAsync(new UninstallInfrastructureTaskRequestEvent(taskId));

            return TaskRequestResponse.TaskRequestAccepted(taskId.Value, "Uninstall infrastructure request accepted");
        }


        private async Task<NextPipeTask> IsInfrastructureInstallRequestRunning()
        {
            var result = await _tasksRepository.GetTasksByTaskType(TaskType.RabbitInfrastructureDeploy);
            if (result.Any())
            {
                foreach (var nextPipeTask in result)
                {
                    if (nextPipeTask.QueueStatus != QueueStatus.Completed)
                    {
                        // This means that there is a RabbitInfrastructureDeploy type which is either pending or running. Reply with an AttachingToProcess msg
                        return nextPipeTask;
                    }   
                }
            }

            return null;
        }

        private async Task<NextPipeTask> IsInfrastructureUninstallRequestRunning()
        {
            var result = await _tasksRepository.GetTasksByTaskType(TaskType.RabbitInfrastructureUninstall);
            if (result.Any())
            {
                foreach (var nextPipeTask in result)
                {
                    if (nextPipeTask.QueueStatus != QueueStatus.Completed)
                    {
                        // A cleanup task is already running attach to that task instead
                        return nextPipeTask;
                    } 
                }
            }

            return null;
        }
    }
}