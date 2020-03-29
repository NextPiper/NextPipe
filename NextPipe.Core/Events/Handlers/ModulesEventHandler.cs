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
using NextPipe.Persistence.Entities.ArchivedObjects;
using NextPipe.Persistence.Entities.NextPipeModules;
using NextPipe.Persistence.Repositories;
using SimpleSoft.Mediator;
using TaskStatus = NextPipe.Persistence.Entities.TaskStatus;

namespace NextPipe.Core.Events.Handlers
{
    public class ModulesEventHandler : 
        IEventHandler<InstallPendingModulesEvent>,
        IEventHandler<InstallModuleEvent>,
        IEventHandler<UninstallModuleEvent>,
        IEventHandler<CleanModulesReadyForUninstallEvent>
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly ITasksRepository _tasksRepository;
        private readonly IModuleManager _moduleManager;
        private readonly IArchiveRepository _archiveRepository;
        
        public ModulesEventHandler(IModuleRepository moduleRepository, ITasksRepository tasksRepository, IModuleManager moduleManager, IArchiveRepository archiveRepository)
        {
            _moduleRepository = moduleRepository;
            _tasksRepository = tasksRepository;
            _moduleManager = moduleManager;
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
        
        public async Task HandleAsync(CleanModulesReadyForUninstallEvent evt, CancellationToken ct)
        {
            LogHandler.WriteLineVerbose($"{nameof(CleanModulesReadyForUninstallEvent)} received - Search and install all modules of {nameof(ModuleStatus)}.{nameof(ModuleStatus.Uninstall)}");

            var result = await _moduleRepository.GetModulesByModuleStatus(ModuleStatus.Uninstall);

            if (!result.Any())
            {
                LogHandler.WriteLineVerbose($"No modules are waiting to be uninstalled - Exiting {nameof(CleanModulesReadyForUninstallEvent)}");
                return;
            }
            
            // Uninstall the modules of status uninstall
            foreach (var module in result)
            {
                await HandleAsync(new UninstallModuleEvent(new Id(module.Id)), ct);
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
                _moduleManager.AttachPreviousLogs(task.Logs);
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
            
            _moduleManager.SetVerboseLogging(true);
            await _moduleManager.DeployModule(new ModuleManagerConfig(
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

        public async Task HandleAsync(UninstallModuleEvent evt, CancellationToken ct)
        {
            var removeModule = await _moduleRepository.GetById(evt.ModuleId.Value);
            await _moduleRepository.UpdateModuleStatus(removeModule.Id, ModuleStatus.Uninstalling);
            Console.WriteLine("Uninstall that module or attach to task for restart");
            NextPipeTask task;
            if (evt.TaskId != null)
            {
                task = await _tasksRepository.GetTaskByTaskId(evt.TaskId.Value);
                _moduleManager.AttachPreviousLogs(task.Logs);
            }
            else
            {
                task = new NextPipeTask
                {
                    CreatedAt = DateTime.Now,
                    EditedAt = DateTime.Now,
                    Id = new Id().Value,
                    Hostname = new Hostname().Value,
                    ReferenceId = evt.ModuleId.Value,
                    TaskId = new Id().Value,
                    TaskStatus = TaskStatus.Running,
                    TaskType = TaskType.ModuleUninstall,
                    QueueStatus = QueueStatus.Running,
                    Metadata = null
                };
                await _tasksRepository.Insert(task);
            }
            _moduleManager.SetVerboseLogging(true);

            await _moduleManager.UninstallModule(new ModuleManagerConfig(evt.TaskId, new ModuleReplicas(removeModule.ModuleReplicas), 
                new ModuleName(removeModule.ModuleName), new ImageName(removeModule.ImageName),
                async (id, logHandler) =>
                {
                    await _archiveRepository.Insert(new ArchiveObject // ask if this is alright of course method should be made, but is it alright to change in the database here 
                    {
                        Id = removeModule.Id,
                        ArchiveReason = ReasonForArchive.Uninstalled,
                        CreatedAt = DateTime.Now,
                        Type = NextPipeObjectType.Module,
                    });
                    await _moduleRepository.Delete(removeModule.Id);
                    await _tasksRepository.FinishTask(task.TaskId, TaskStatus.Success, logHandler.GetLog());

                },
                async (id, LogHandler) =>
                {
                    await _moduleRepository.SetModuleStatusUninstall(removeModule.Id);
                    await _tasksRepository.FinishTask(task.TaskId, TaskStatus.Failed, LogHandler.GetLog());
                },
                async (id, logHandler) => { await _tasksRepository.AppendLog(task.TaskId, logHandler.GetLog()); }));

        }
    }
}