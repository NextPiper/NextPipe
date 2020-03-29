using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NextPipe.Core.Domain.NextPipeTask.ValueObject;
using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Core.Events.Events;
using NextPipe.Core.Events.Events.ModuleEvents;
using NextPipe.Core.Helpers;
using NextPipe.Core.Kubernetes;
using NextPipe.Core.ValueObjects;
using NextPipe.Messaging.Infrastructure.Contracts;
using NextPipe.Persistence.Entities;
using NextPipe.Persistence.Entities.ArchivedObjects;
using NextPipe.Persistence.Entities.Metadata;
using NextPipe.Persistence.Entities.NextPipeModules;
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
        private readonly IModuleRepository _moduleRepository;
        private readonly IArchiveRepository _archiveRepository;
        private const string NEXTPIPE_DEPLOYMENT = "nextpipe-deployment";

        public ResourceAndStateCleanEventHandler(ITasksRepository tasksRepository, IKubectlHelper kubectlHelper, IEventPublisher eventPublisher, IModuleRepository moduleRepository, IArchiveRepository archiveRepository)
        {
            _tasksRepository = tasksRepository;
            _kubectlHelper = kubectlHelper;
            _eventPublisher = eventPublisher;
            _moduleRepository = moduleRepository;
            _archiveRepository = archiveRepository;
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
                case TaskType.ModuleInstall:
                    await CleanUpModuleInstallTasks(task);
                    break;
                case TaskType.ModuleUninstall:
                    await CleanUpModuleUninstallTasks(task);
                    break;
                default:
                     return;
            }
        }

        private async Task CleanUpModuleUninstallTasks(NextPipeTask task)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Task: {task.TaskId} of type: {task.TaskType} is scheduled on dead host: {task.Hostname} - Re-Attaching task to live host");
            
            // Get overview of the state which the task was left in...
            var module = await _moduleRepository.GetById(task.ReferenceId);

            // The module is no longer present and is such not a part of the system anymore, the uninstallTask is thus by definition done
            if (module == null)
            {
                builder.AppendLine(
                    $"ModuleId: {task.ReferenceId} can't be found for {task.TaskType} - taskId: {task.TaskId} terminating task with {nameof(TaskStatus)}.{nameof(TaskStatus.Failed)}");
                // why is the module null ?? was it uninstalled and then removed but the task hanging? In that case the module should have been archived under archive type module with the modules id
                // as typeReferenceId... See if we can find the module there. In that case the task was in principle successful. If we can't find the moduleReference there it can be difficult to
                // determine why it is null and if the module has even been deleted from kubernetes, mark the task as failed to let the user know.
                var archive = await _archiveRepository.GetArchiveByTypeAndReferenceId(module.Id, NextPipeObjectType.Module);
                if (archive != null)
                {
                    await _tasksRepository.FinishTask(task.TaskId, TaskStatus.Success, task.Logs + builder.ToString());
                    return;
                }
                await _tasksRepository.FinishTask(task.TaskId, TaskStatus.Failed,task.Logs + builder.ToString());
                return;
            }

            if (module.ModuleStatus == ModuleStatus.Uninstalled)
            {
                // The respective module made it into ModuleStatus.Uninstall, in this case the task was indeed successful.
                builder.AppendLine($"The associated module withId:  {module.Id} managed to be uninstalled. Terminating task with {nameof(TaskStatus)}.{TaskStatus.Success}");
                await _tasksRepository.FinishTask(task.TaskId, TaskStatus.Success, task.Logs + builder.ToString());
                return;
            }
            
            if (module.ModuleStatus == ModuleStatus.FailedUninstall)
            {
                // The respective module managed to run uninstall() but encountered a failedUninstall --> The task should thus fail and the user should take action
                builder.AppendLine(
                    $"The associated module withId: {module.Id} managed to be uninstalled. Terminating task with {nameof(TaskStatus)}.{TaskStatus.Failed}");
                await _tasksRepository.FinishTask(task.TaskId, TaskStatus.Failed,task.Logs + builder.ToString());
                return;
            }

            // The module is still existing, and didn't reach an uninstalled state or failedUninstallState try restarting the the task if its restart limit has not been reached
            if (task.Restarts >= 1)
            {
                // We already went through one restart, mark the module as failedUninstall and the task as failed
                builder.AppendLine("Suspending task... Restart limit 1/1 reached. See logs for failure reason and check if manuel cleanup is required");
                await _moduleRepository.UpdateModuleStatus(module.Id, ModuleStatus.FailedUninstall);
                await _tasksRepository.FinishTask(task.TaskId, TaskStatus.Failed, task.Logs + builder.ToString());
                return;
            }
            
            // Restart the task in order to see if we can resolve the issues from previous dead host
            var host = new Hostname();
            builder.AppendLine(
                $"Task was restarted due to previous host death: Restart 1/1 - Attaching task to host: {host.Value}");
            await _tasksRepository.IncrementRestarts(task.TaskId, host.Value, task.Logs + builder.ToString());
            await _eventPublisher.PublishAsync(new UninstallModuleEvent(new Id(module.Id), new Id(task.TaskId)));
        }
        
        private async Task CleanUpModuleInstallTasks(NextPipeTask task)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"Task: {task.TaskId} of type: {task.TaskType} is scheduled on dead host: {task.Hostname} - Doing Cleanup");
            // Check if the reference module was resolved to either running or failed, if so resolve the task into state failed/success respectively
            var module = await _moduleRepository.GetById(task.ReferenceId);
            
            if (module == null)
            {
                // The module reference was null, an is such not a part of the system anymore, the installTask is thus by definition done and failed
                builder.AppendLine(
                    $"ModuleId: {task.ReferenceId} can't be found for {task.TaskType} - taskId: {task.TaskId} terminating task with {nameof(TaskStatus)}.{nameof(TaskStatus.Failed)}");
                await _tasksRepository.FinishTask(task.TaskId, TaskStatus.Failed, task.Logs + builder.ToString());
                return;
            }

            // Check if the task merely needs to be closed based on module status
            switch (module.ModuleStatus)
            {
                case ModuleStatus.Running:
                    builder.AppendLine(
                        $"The associated module with id:{module.Id} managed to get into a running state. Terminating task with {nameof(TaskStatus)}.{nameof(TaskStatus.Success)}");
                    await _tasksRepository.FinishTask(task.TaskId, TaskStatus.Success, task.Logs + builder.ToString());
                    return;
                case ModuleStatus.Failed:
                    builder.AppendLine(
                        $"The associated module with id:{module.Id} is in failed state. Terminating task with {nameof(TaskStatus)}.{nameof(TaskStatus.Success)}");
                    await _tasksRepository.FinishTask(task.TaskId, TaskStatus.Failed, task.Logs + builder.ToString());
                    return;
            }
            
            if (task.Restarts >= 1)
            {
                // We already went through one restart. However the we dont know exactly why it will not install or if it is safe to clean up
                // fx. if the restart failed because a resource already exists in kubernetes, we do not neccesarily know if that resource is safe to delete
                // the user will have to see the logs in order to resolve the issue
                builder.AppendLine("Suspending task... Restart limit 1/1 reached. See logs for failure reason and check if manuel cleanup is required");
                await _moduleRepository.UpdateModuleStatus(module.Id, ModuleStatus.Failed);
                await _tasksRepository.FinishTask(task.TaskId, TaskStatus.Failed, task.Logs + builder.ToString());
                return;
            }
            
            // Restart the task in order to see if we can resolve the issues from previous dead host
            var host = new Hostname();
            builder.AppendLine($"Task was restarted due to previous host death. Restart 1/1 - Attaching task to host: {host.Value}");
            await _tasksRepository.IncrementRestarts(task.TaskId, host.Value, task.Logs + builder.ToString());
            await _eventPublisher.PublishAsync(new InstallModuleEvent(new Id(module.Id), new Id(task.Id)));

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