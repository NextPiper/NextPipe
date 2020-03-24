using System;
using System.Threading;
using System.Threading.Tasks;
using NextPipe.Core.Commands.Commands.ModuleCommands;
using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Core.Events.Events;
using NextPipe.Messaging.Infrastructure.Contracts;
using NextPipe.Persistence.Entities;
using NextPipe.Persistence.Entities.NextPipeModules;
using NextPipe.Persistence.Repositories;
using NextPipe.Utilities.Documents.Responses;
using SimpleSoft.Mediator;
using TaskStatus = NextPipe.Persistence.Entities.TaskStatus;

namespace NextPipe.Core.Commands.Handlers
{
    public class ModuleCommandHandler : CommandHandlerBase, 
        ICommandHandler<RequestInstallModule, TaskRequestResponse>
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly ITasksRepository _tasksRepository;

        public ModuleCommandHandler(IEventPublisher eventPublisher, IModuleRepository moduleRepository, ITasksRepository tasksRepository) : base(eventPublisher)
        {
            _moduleRepository = moduleRepository;
            _tasksRepository = tasksRepository;
        }

        public async Task<TaskRequestResponse> HandleAsync(RequestInstallModule cmd, CancellationToken ct)
        {
            var imageResult = await _moduleRepository.GetModuleByImageName(cmd.ImageName.Value);
            var moduleNameResult = await _moduleRepository.GetModuleByModuleName(cmd.ModuleName.Value);
            
            // The module which is being requested for install has a image which is already in the system.
            if (!imageResult.Equals(null) && moduleNameResult.Equals(null))
            {
                return new TaskRequestResponse(imageResult.Id, $"The module requested to be installed is already present in a different deployment. The image --insertimage is already running in --insertmodulename  ",false);
            }
            // The module which is being requested for install has a duplicate deployment name.
            if (imageResult.Equals(null) && !moduleNameResult.Equals(null))
            {
                return new TaskRequestResponse(moduleNameResult.Id, $"The module requested to be installed has a duplicate deployment name. {cmd.ModuleName} is already taken",false);
            }
            //The module which is being requested for install is already in the system.
            if (!imageResult.Equals(null) && !moduleNameResult.Equals(null))
            {
                return new TaskRequestResponse(imageResult.Id, $"The module requested to be installed is already in the system. {cmd.ImageName} and {cmd.ModuleName} has status: --insertstatus and this amountofreplicas ",false);
            }
            
            var moduleId = new Id();
            await _moduleRepository.Insert(new Module
            {
                CreatedAt = DateTime.Now,
                EditedAt = DateTime.Now,
                Id = moduleId.Value,
                ModuleStatus = ModuleStatus.Pending,
                ImageName = cmd.ImageName.Value,
                ModuleName = cmd.ModuleName.Value,
                ModuleReplicas = cmd.ModuleReplicas.Value
            });
            
            var taskId = new Id();
            await _tasksRepository.Insert(new NextPipeTask
            {
                CreatedAt = DateTime.Now,
                EditedAt = DateTime.Now,
                Id = new Id().Value,
                QueueStatus = QueueStatus.Pending,
                TaskId = taskId.Value,
                TaskStatus = TaskStatus.Ready,
                TaskPriority = TaskPriority.Medium,
                TaskType = TaskType.ModuleInstall,
                Logs = "",
                ReferenceId = moduleId.Value,
            });
            
            _eventPublisher.PublishAsync(
                new InstallModuleTaskRequestEvent(taskId, cmd.ModuleReplicas, cmd.ImageName, cmd.ModuleName));
            return TaskRequestResponse.TaskRequestAccepted(taskId.Value, "Module Installation Request Accepted");

        }
    }
}