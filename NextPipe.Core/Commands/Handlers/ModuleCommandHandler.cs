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
        ICommandHandler<RequestInstallModule, TaskRequestResponse>,
        ICommandHandler<RequestDeleteModuleCommand, Response>,
        ICommandHandler<ScaleModuleCommand, Response>
    {
        private readonly IModuleRepository _moduleRepository;

        public ModuleCommandHandler(IEventPublisher eventPublisher, IModuleRepository moduleRepository, ITasksRepository tasksRepository) : base(eventPublisher)
        {
            _moduleRepository = moduleRepository;
        }

        public async Task<TaskRequestResponse> HandleAsync(RequestInstallModule cmd, CancellationToken ct)
        {

            var imageResult = await _moduleRepository.GetModuleByImageName(cmd.ImageName.Value);
            var moduleNameResult = await _moduleRepository.GetModuleByModuleName(cmd.ModuleName.Value);

            // The module which is being requested for install has a image which is already in the system.
            if (!(imageResult is null) && moduleNameResult is null)
            {
                return new TaskRequestResponse(imageResult.Id, $"The module requested to be installed is already present in a different deployment. The image {imageResult.ImageName} is already running in {imageResult.ModuleName}",false);
            }
            // The module which is being requested for install has a duplicate deployment name.
            if (imageResult is null && !(moduleNameResult is null))
            {
                return new TaskRequestResponse(moduleNameResult.Id, $"The module requested to be installed has a duplicate deployment name. {cmd.ModuleName} is already taken",false);
            }
            //The module which is being requested for install is already in the system.
            if (!(imageResult is null) && !(moduleNameResult is null))
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
                DesiredReplicas = cmd.ModuleReplicas.Value
            });
            
            return TaskRequestResponse.TaskRequestAccepted(moduleId.Value, "Module was accepted and is awaiting upstart");
        }

        public async Task<Response> HandleAsync(RequestDeleteModuleCommand cmd, CancellationToken ct)
        {
            var module = await _moduleRepository.GetById(cmd.Id.Value);

            if (module.ModuleStatus == ModuleStatus.Installing || module.ModuleStatus == ModuleStatus.Uninstalling)
            {
                return Response.Unsuccessful("Can't delete module while it is installing or uninstalling. Wait until ModulStatus changes");
            }
            
            var result = await _moduleRepository.SetModuleStatusUninstall(cmd.Id.Value);

            if (result == null)
            {
                return Response.Unsuccessful();
            }
            
            return Response.SuccesMsg($"Request accepted, deleting module with id: {cmd.Id}");
        }

        public async Task<Response> HandleAsync(ScaleModuleCommand cmd, CancellationToken ct)
        {
            var module = await _moduleRepository.GetById(cmd.Id.Value);

            if (module.ModuleStatus !=  ModuleStatus.Running)
            {
                return Response.Unsuccessful("Can't scale module with a moduleStatus different than running");
            }
            
            var result = await _moduleRepository.UpdateDesiredReplicas(cmd.Id.Value, cmd.Replicas.Value);

            if (result == null)
            {
                return Response.Unsuccessful();
            }
            
            return Response.Success();
        }
    }
}