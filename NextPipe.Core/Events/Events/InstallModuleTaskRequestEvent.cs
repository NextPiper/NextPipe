using System.Security.Cryptography.X509Certificates;
using NextPipe.Core.Domain.Module.ValueObjects;
using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Persistence.Entities.NextPipeModules;

namespace NextPipe.Core.Events.Events
{
    public class InstallModuleTaskRequestEvent : BaseEvent
    {
        public Id TaskId { get; }
        
        public Id ReferenceId { get; }
        public ModuleReplicas ModuleReplicas { get; }
        public ImageName ImageName { get; }
        public ModuleName ModuleName { get; }

        public InstallModuleTaskRequestEvent(Id taskId, Id referenceId, ModuleReplicas moduleReplicas, ImageName imageName,
            ModuleName moduleName)
        {
            TaskId = taskId;
            ReferenceId = referenceId;
            ModuleReplicas = moduleReplicas;
            ImageName = imageName;
            ModuleName = moduleName;
        }
    }
    
}