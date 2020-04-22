using System;

namespace NextPipe.Persistence.Entities.ProcessLock
{
    public class ProcessLock : IEntity
    {
        public Guid Id { get; set; }
        public Guid ProcessId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime EditedAt { get; set; } = DateTime.Now;
        
        // The hostname of the respective machine locking this process
        public string Hostname { get; set; }
        public string NextPipeProcessType { get; set; }
    }
}