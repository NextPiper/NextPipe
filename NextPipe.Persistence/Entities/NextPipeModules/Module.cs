using System;

namespace NextPipe.Persistence.Entities.NextPipeModules
{
    public class Module : BaseArchivable, IEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime EditedAt { get; set; } = DateTime.Now;
        
        public string ImageName { get; set; }
        public string ModuleName { get; set; }
        public ModuleStatus ModuleStatus { get; set; }
        public int ModuleReplicas { get; set; }
        public string Logs { get; set; } = "";

    }
}