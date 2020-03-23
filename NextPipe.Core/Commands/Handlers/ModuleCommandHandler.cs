using System;
using System.Threading;
using System.Threading.Tasks;
using NextPipe.Core.Commands.Commands.ModuleCommands;
using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Core.Events.Events;
using NextPipe.Messaging.Infrastructure.Contracts;
using NextPipe.Persistence.Entities.NextPipeModules;
using NextPipe.Persistence.Repositories;
using NextPipe.Utilities.Documents.Responses;
using SimpleSoft.Mediator;

namespace NextPipe.Core.Commands.Handlers
{
    public class ModuleCommandHandler : CommandHandlerBase, 
        ICommandHandler<RequestInstallModule, TaskRequestResponse>
    {
        private readonly IModuleRepository _moduleRepository;

        public ModuleCommandHandler(IEventPublisher eventPublisher, IModuleRepository moduleRepository) : base(eventPublisher)
        {
            _moduleRepository = moduleRepository;
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
                Id = Guid.NewGuid(),
                ImageName = cmd.ImageName.Value,
                ModuleName = cmd.ModuleName.Value,
                ModuleReplicas = cmd.ModuleReplicas.Value

            });
            _eventPublisher.PublishAsync(
                new InstallModuleTaskRequestEvent(moduleId, cmd.ModuleReplicas, cmd.ImageName, cmd.ModuleName));
            return TaskRequestResponse.TaskRequestAccepted(moduleId.Value, "Module Installation Request Accepted");

        }
    }
}