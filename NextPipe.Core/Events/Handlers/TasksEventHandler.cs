using System;
using System.Threading;
using System.Threading.Tasks;
using NextPipe.Core.Domain.Module.ModuleManagers;
using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Core.Events.Events;
using NextPipe.Core.Helpers;
using NextPipe.Persistence.Entities;
using NextPipe.Persistence.Entities.NextPipeModules;
using NextPipe.Persistence.Repositories;
using SimpleSoft.Mediator;
using TaskStatus = NextPipe.Persistence.Entities.TaskStatus;

namespace NextPipe.Core.Events.Handlers
{
    public class TasksEventHandler : IEventHandler<InitializeInfrastructureTaskRequestEvent>,IEventHandler<InstallModuleTaskRequestEvent>
    public class TasksEventHandler : 
        IEventHandler<InitializeInfrastructureTaskRequestEvent>,
        IEventHandler<UninstallInfrastructureTaskRequestEvent>
    {
        private readonly ITasksRepository _tasksRepository;
        private readonly IRabbitDeploymentManager _rabbitDeploymentManager;
        private readonly IModuleRepository _moduleRepository;
        private readonly IModuleInstallManager _moduleInstallManager;

        public TasksEventHandler(ITasksRepository _tasksRepository, IRabbitDeploymentManager rabbitDeploymentManager, IModuleRepository moduleRepository)
        {
            this._tasksRepository = _tasksRepository;
            _rabbitDeploymentManager = rabbitDeploymentManager;
            _moduleRepository = moduleRepository;
        }

        /// <summary>
        /// Provisions the infrastructure
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task HandleAsync(InitializeInfrastructureTaskRequestEvent evt, CancellationToken ct)
        {
            // Start by updating the que and task status appropriately 
            await _tasksRepository.UpdateTaskQueueStatus(evt.TaskId.Value, QueueStatus.Running);
            await _tasksRepository.UpdateTaskStatus(evt.TaskId.Value, TaskStatus.Running);
            
            // Make sure the logging is verbose for console prints
            _rabbitDeploymentManager.SetVerboseLogging(true);
            
            // Deploy rabbitMQ infrastructure
            await _rabbitDeploymentManager.Deploy(
                new RabbitDeploymentManagerConfiguration(evt.TaskId, evt.LowerBoundaryReadyReplicas, evt.ReplicaFailureThreshold,
                    evt.ReplicaDelaySeconds, evt.RabbitNumberOfReplicas, SuccessCallback, FailureCallback, UpdateCallback));
        }
        
        public async Task HandleAsync(UninstallInfrastructureTaskRequestEvent evt, CancellationToken ct)
        {
            // Start by updating the que and task status appropriately
            await _tasksRepository.UpdateTaskQueueStatus(evt.TaskId.Value, QueueStatus.Running);
            await _tasksRepository.UpdateTaskStatus(evt.TaskId.Value, TaskStatus.Running);
            
            _rabbitDeploymentManager.AttachTaskIdAndUpdateHandler(evt.TaskId, UpdateCallback);

            await _rabbitDeploymentManager.Cleanup(SuccessCallback, FailureCallback);
        }
        
        private async Task SuccessCallback(Id taskId, ILogHandler logHandler)
        {
            await _tasksRepository.FinishTask(taskId.Value, TaskStatus.Success, logHandler.GetLog());
        }

        private async Task UpdateCallback(Id taskId, ILogHandler logHandler)
        {
            await _tasksRepository.AppendLog(taskId.Value, logHandler.GetLog());
        }

        private async Task FailureCallback(Id taskId, ILogHandler logHandler)
        {
            await _tasksRepository.FinishTask(taskId.Value, TaskStatus.Failed, logHandler.GetLog());
        }

        public async Task HandleAsync(InstallModuleTaskRequestEvent evt, CancellationToken ct)
        {
            await _moduleRepository.UpdateModuleStatus(evt.Id.Value, ModuleStatus.Installing);

            await _moduleInstallManager.DeployModule(new ModuleInstallManagerConfig(evt.Id, evt.ModuleReplicas,
                evt.ModuleName, evt.ImageName, SuccessCallback, FailureCallback, UpdateCallback));
        }
    }
}
