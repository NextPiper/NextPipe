using System;
using NextPipe.Core.Domain.Module.ValueObjects;
using NextPipe.Utilities.Documents.Responses;
using SimpleSoft.Mediator;

namespace NextPipe.Core.Commands.Commands.ModuleCommands
{
    public class RequestInstallModule : Command<TaskRequestResponse>
    {
        public ImageName ImageName { get; }
        public ModuleReplicas ModuleReplicas { get; }
        public ModuleName ModuleName { get; }
        
        public RequestInstallModule(string imageName, int moduleReplicas, string moduleName)
        {
            ImageName = new ImageName(imageName);
            ModuleReplicas = new ModuleReplicas(moduleReplicas);
            ModuleName = new ModuleName(moduleName);
        }
    }
}