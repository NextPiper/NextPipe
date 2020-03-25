using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NextPipe.Core.Domain.NextPipeTask.ValueObject;
using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Core.Events.Events;
using NextPipe.Core.Helpers;
using NextPipe.Core.Kubernetes;
using NextPipe.Core.ValueObjects;
using NextPipe.Messaging.Infrastructure.Contracts;
using NextPipe.Persistence.Entities;
using NextPipe.Persistence.Entities.Metadata;
using NextPipe.Persistence.Repositories;
using SimpleSoft.Mediator;
using TaskStatus = NextPipe.Persistence.Entities.TaskStatus;

namespace NextPipe.Core.Events.Handlers
{
    public class ResourceAndStateCleanEventHandler : IEventHandler<CleanupHangingTasksEvent>
    {
        private readonly ITasksRepository _tasksRepository;
        private readonly IKubectlHelper _kubectlHelper;
        private readonly IEventPublisher _eventPublisher;
        private const string NEXTPIPE_DEPLOYMENT = "nextpipe-deployment";

        public ResourceAndStateCleanEventHandler(ITasksRepository tasksRepository, IKubectlHelper kubectlHelper, IEventPublisher eventPublisher)
        {
            _tasksRepository = tasksRepository;
            _kubectlHelper = kubectlHelper;
            _eventPublisher = eventPublisher;
        }
        
        public async Task HandleAsync(CleanupHangingTasksEvent evt, CancellationToken ct)
        {
            
            // Find all running tasks in system
            var runningTasks = await _tasksRepository.GetAllRunningTasks(0, 10000);
            LogHandler.WriteLineVerbose($"Fetching running tasks: {runningTasks.Count()}");
            // If there is any running task in the system ensure that they are not hanging by matching their hostname
            // with all currently running pods...
            if (runningTasks.Any())
            {
                // Get all running nextpipe-deployment hosts
                var hosts = await _kubectlHelper.GetPodsByCustomNameFilter(NEXTPIPE_DEPLOYMENT,
                    ShellHelper.IdenticalStart);

                //Iterate each task and if the tasks host is not running clean up the task
                foreach (var task in runningTasks)
                {
                    if (!hosts.Any(t => t.Metadata.Name.Trim().ToLower().Equals(task.Hostname.Trim().ToLower())))
                    {
                        LogHandler.WriteLineVerbose($"Task: {task.TaskId} of type: {task.TaskType} is scheduled on dead host: {task.Hostname} - Doing Cleanup");
                        await CleanupTask(task);
                    }
                }
            }
        }

        private async Task CleanupTask(NextPipeTask task)
        {
            switch (task.TaskType)
            {
                case TaskType.RabbitInfrastructureDeploy:
                    await CleanUpRabbitInfrastructureDeploy(task);
                    break;
                case TaskType.RabbitInfrastructureUninstall:
                    await CleanUpRabbitInfrastructureUninstall(task);
                    break;
                default:
                     return;
            }
        }

        private async Task CleanUpRabbitInfrastructureDeploy(NextPipeTask task)
        {
            var builder = new StringBuilder();
            
            var metadata = task.Metadata as InfrastructureInstallMetadata;
            // The task is hanging --> if the number of restarts is 1 then cleanup the rabbitInfrastructure task
            // if metadata is null then also cleanup, we dont have required data to do a restart
            if (task.Restarts >= 1 || metadata == null)
            {
                LogHandler.WriteLineVerbose($"Task has already been restarted {task.Restarts} time(s) or metadata is null");
                // Task already restarted once, stop task and cleanup infrastructure
                builder.AppendLine("Suspending task... Restart limit 1/1 reached. See logs for failure reason and check if manuel cleanup is required");
                await _tasksRepository.AppendLog(task.TaskId, task.Logs + builder.ToString());
                await _tasksRepository.UpdateStatus(task.TaskId, TaskStatus.Failed, QueueStatus.Suspended);
                
                // Schedule an uninstallInfrastructure cleanup
                var id = new Id();
                await _tasksRepository.Insert(new NextPipeTask
                {
                    Id = new Id().Value,
                    TaskId = id.Value,
                    TaskStatus = TaskStatus.Ready,
                    QueueStatus = QueueStatus.Pending,
                    TaskType = TaskType.RabbitInfrastructureUninstall,
                    TaskPriority = TaskPriority.Fatal,
                    ReferenceId = task.TaskId,
                    Hostname = new Hostname().Value,
                });
                
                await _eventPublisher.PublishAsync(new UninstallInfrastructureTaskRequestEvent(id));
                return;
            }

            var host = new Hostname();
            LogHandler.WriteLineVerbose($"Task was restarted due to previous host death. Restart 1/1 - Attaching task to host: {host.Value}");
            builder.AppendLine($"Task was restarted due to previous host death. Restart 1/1 - Attaching task to host: {host.Value}");
            // Increment restarts and log
            await _tasksRepository.IncrementRestarts(task.TaskId, host.Value, task.Logs + builder.ToString());
            LogHandler.WriteLineVerbose("Task was updated with new host.Value and logs");
            // Republish infrastructure initialize event
            await _eventPublisher.PublishAsync(new InitializeInfrastructureTaskRequestEvent(
                new Id(task.TaskId),
                new LowerBoundaryReadyReplicas(metadata.LowerBoundaryReadyReplicas),
                new ReplicaFailureThreshold(metadata.ReplicaFailureThreshold),
                new ReplicaDelaySeconds(metadata.ReplicaDelaySeconds),
                new RabbitNumberOfReplicas(metadata.RabbitNumberOfReplicas)));
        }

        private async Task CleanUpRabbitInfrastructureUninstall(NextPipeTask task)
        {
            var builder = new StringBuilder();
            
            if (task.Restarts >= 2)
            {
                builder.AppendLine(
                    "Suspending task... Restart limit 2/2 reached. See logs for failure and do manuel cleanup");
                await _tasksRepository.AppendLog(task.TaskId, task.Logs + builder.ToString());
                await _tasksRepository.UpdateTaskStatus(task.TaskId, TaskStatus.Failed);
                await _tasksRepository.UpdateTaskQueueStatus(task.TaskId, QueueStatus.Suspended);
                return;
            }

            builder.AppendLine(
                $"Task was restarted due to previous host death complications. Restart {task.Restarts + 1}/2");
            
            await _tasksRepository.IncrementRestarts(task.TaskId, new Hostname().Value, task.Logs + builder.ToString());
            await _eventPublisher.PublishAsync(new UninstallInfrastructureTaskRequestEvent(new Id(task.Id)));
        }
    }
}