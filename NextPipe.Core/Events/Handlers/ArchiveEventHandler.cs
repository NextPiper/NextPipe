using System;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.IO;
using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Core.Events.Events.ArchiveEvents;
using NextPipe.Persistence.Entities;
using NextPipe.Persistence.Entities.ArchivedObjects;
using NextPipe.Persistence.Entities.NextPipeModules;
using NextPipe.Persistence.Repositories;
using SimpleSoft.Mediator;

namespace NextPipe.Core.Events.Handlers
{
    public class ArchiveEventHandler : 
        IEventHandler<ArchiveModuleEvent>,
        IEventHandler<ArchiveModulesEvent>,
        IEventHandler<ArchiveTasksEvent>
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly IArchiveRepository _archiveRepository;
        private readonly ITasksRepository _tasksRepository;

        public ArchiveEventHandler(IModuleRepository moduleRepository, IArchiveRepository archiveRepository, ITasksRepository tasksRepository)
        {
            _moduleRepository = moduleRepository;
            _archiveRepository = archiveRepository;
            _tasksRepository = tasksRepository;
        }

        public async Task HandleAsync(ArchiveModuleEvent evt, CancellationToken ct)
        {
            var module = await _moduleRepository.GetById(evt.ModuleId.Value);

            if (module != null)
            {
                if (module.ModuleStatus == ModuleStatus.Uninstalled)
                {
                    // To ensure no dataloss, we will insert the archive first and make the insert idempotent
                    await ArchiveModule(module);
                }
            }
        }

        public async Task HandleAsync(ArchiveModulesEvent evt, CancellationToken ct)
        {
            var modulesReadyForArchive = await _moduleRepository.GetModulesByModuleStatus(ModuleStatus.Uninstalled);

            foreach (var module in modulesReadyForArchive)
            {
                // To ensure no dataloss, we will insert the archive first and make the insert idempotent
                await ArchiveModule(module);
            }
        }

        public async Task HandleAsync(ArchiveTasksEvent evt, CancellationToken ct)
        {
            var completedTasks = await _tasksRepository.GetCompletedTasks();

            foreach (var task in completedTasks)
            {
                await ArchiveTask(task);
            }
        }

        private async Task ArchiveTask(NextPipeTask task)
        {
            var insertSuccessful = false;
            var archiveId = new Id();
            // First try to archive to prevent data loss
            try
            {
                await _archiveRepository.Insert(new ArchiveObject
                {
                    Id = archiveId.Value,
                    Type = NextPipeObjectType.NextPipeTask,
                    TypeReferenceId = task.Id,
                    ArchiveReason = ReasonForArchive.TaskCompleted,
                    Metadata = task
                });
                insertSuccessful = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                insertSuccessful = false;
            }

            if (insertSuccessful)
            {
                try
                {
                    await _tasksRepository.Delete(task.Id);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    await _archiveRepository.Delete(archiveId.Value);
                }
            }
        }

        private async Task ArchiveModule(Module module)
        {
            var insertSuccessful = false;
            var archiveId = new Id();
            try
            {
                await _archiveRepository.Insert(new ArchiveObject
                {
                    Id = archiveId.Value,
                    Type = NextPipeObjectType.Module,
                    TypeReferenceId = module.Id,
                    ArchiveReason = ReasonForArchive.Uninstalled,
                    Metadata = module
                });
                insertSuccessful = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                insertSuccessful = false;
            }

            if (insertSuccessful)
            {
                try
                {
                    await _moduleRepository.Delete(module.Id);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    // Fallback and delete the inserted archive
                    await _archiveRepository.Delete(archiveId.Value);
                }
            }
        }
    }
}