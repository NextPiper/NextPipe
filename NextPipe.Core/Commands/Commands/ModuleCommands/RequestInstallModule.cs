using System;
using NextPipe.Core.Domain.Module.ValueObjects;
using NextPipe.Utilities.Documents.Responses;
using SimpleSoft.Mediator;

namespace NextPipe.Core.Commands.Commands.ModuleCommands
{
    public class RequestInstallModule : Command<TaskRequestResponse>
    {
        public ImageName ImageName { get; }
        public ImageReplicas ImageReplicas { get; }
        public ModuleName ModuleName { get; }
        
        public RequestInstallModule(string imageName, int imageReplicas, string moduleName)
        {
            ImageName = new ImageName(imageName);
            ImageReplicas = new ImageReplicas(imageReplicas);
            ModuleName = new ModuleName(moduleName);
        }
    }
}