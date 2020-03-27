using System;
using NextPipe.Core.Domain.Module.ValueObjects;
using NextPipe.Core.Domain.SharedValueObjects;

namespace NextPipe.Core.Commands.Commands.ModuleCommands
{
    public class ScaleModuleCommand : BaseCommand
    {
        public Id Id { get; }
        public ModuleReplicas Replicas { get; }
        
        public ScaleModuleCommand(Guid moduleId, int replicas)
        {
            Id = new Id(moduleId);
            Replicas = new ModuleReplicas(replicas);
        }
    }
}