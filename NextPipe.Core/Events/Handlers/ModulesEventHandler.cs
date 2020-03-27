using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NextPipe.Core.Domain.Module.ModuleManagers;
using NextPipe.Core.Domain.Module.ValueObjects;
using NextPipe.Core.Domain.NextPipeTask.ValueObject;
using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Core.Events.Events.ModuleEvents;
using NextPipe.Core.Helpers;
using NextPipe.Persistence.Entities;
using NextPipe.Persistence.Entities.NextPipeModules;
using NextPipe.Persistence.Repositories;
using SimpleSoft.Mediator;
using TaskStatus = NextPipe.Persistence.Entities.TaskStatus;

namespace NextPipe.Core.Events.Handlers
{
    public class ModulesEventHandler : 
        IEventHandler<InstallPendingModulesEvent>,
        IEventHandler<InstallModuleEvent>
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly ITasksRepository _tasksRepository;
        private readonly IModuleInstallManager _moduleInstallManager;

        public ModulesEventHandler(IModuleRepository moduleRepository, ITasksRepository tasksRepository, IModuleInstallManager moduleInstallManager)
        {
            _moduleRepository = moduleRepository;
            _tasksRepository = tasksRepository;
            _moduleInstallManager = moduleInstallManager;
        }
        
        public async Task HandleAsync(InstallPendingModulesEvent evt, CancellationToken ct)
        {
            LogHandler.WriteLineVerbose($"{nameof(InstallPendingModulesEvent)} received - Search and install all modules of {nameof(ModuleStatus)}.{nameof(ModuleStatus.Pending)}");

            var result = await _moduleRepository.GetModulesByModuleStatus(ModuleStatus.Pending);

            if (!result.Any())
            {
                LogHandler.WriteLineVerbose($"No modules are pending for installation - Exiting {nameof(InstallPendingModulesEvent)}");
                return;
            }
            
            // Install the pending modules
            foreach (var module in result)
            {
                await HandleAsync(new InstallModuleEvent(new Id(module.Id)), ct);
            }
        }


        public async Task HandleAsync(InstallModuleEvent evt, CancellationToken ct)
        {
            // Fetch module first
            var module = await _moduleRepository.GetById(evt.ModuleId.Value);
            
            // Update module status to installing
            await _moduleRepository.UpdateModuleStatus(module.Id, ModuleStatus.Installing);
            
            // Create a task to handle this installation
            NextPipeTask task;
            if (evt.TaskId != null) // If the module event comes with previous taskId then attach to that task instead.
            {
                task = await _tasksRepository.GetTaskByTaskId(evt.TaskId.Value);
                _moduleInstallManager.AttachPreviousLogs(task.Logs);
            }
            else
            {
                task = new NextPipeTask
                {
                    Id = new Id().Value,
                    QueueStatus = QueueStatus.Running,
                    TaskId = new Id().Value,
                    TaskStatus = TaskStatus.Running,
                    TaskPriority = TaskPriority.Medium,
                    TaskType = TaskType.ModuleInstall,
                    Hostname = new Hostname().Value,
                    ReferenceId = module.Id,
                    Metadata = null
                };
                await _tasksRepository.Insert(task);
            }
            
            _moduleInstallManager.SetVerboseLogging(true);
            await _moduleInstallManager.DeployModule(new ModuleInstallManagerConfig(
                new Id(task.TaskId), 
                new ModuleReplicas(module.ModuleReplicas),
                new ModuleName(module.ModuleName),
                new ImageName(module.ImageName),
                async (id, logHandler) =>
                {
                    await _moduleRepository.UpdateModuleStatus(module.Id, ModuleStatus.Running);
                    await _tasksRepository.FinishTask(id.Value, TaskStatus.Success, logHandler.GetLog());
                },
                async (id, logHandler) =>
                {
                    await _moduleRepository.UpdateModuleStatus(module.Id, ModuleStatus.Failed);
                    await _tasksRepository.FinishTask(id.Value, TaskStatus.Failed, logHandler.GetLog());
                },
                async (id, logHandler) => { await _tasksRepository.AppendLog(id.Value, logHandler.GetLog()); }));
        }
    }
}