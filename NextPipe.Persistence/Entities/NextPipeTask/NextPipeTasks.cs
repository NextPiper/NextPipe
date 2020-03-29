using System;
using System.Dynamic;

namespace NextPipe.Persistence.Entities
{
    public class NextPipeTask : BaseArchivable, IEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime EditedAt { get; set; } = DateTime.Now;
        // The specific TaskId for cross reference
        public Guid TaskId { get; set; }
        public QueueStatus QueueStatus { get; set; }
        public TaskStatus TaskStatus { get; set; }
        public TaskType TaskType { get; set; }
        public TaskPriority TaskPriority { get; set; }
        public Guid ReferenceId { get; set; }
        public string Logs { get; set; } = "";
        public string Hostname { get; set; }
        public int Restarts { get; set; } = 0;
        public BaseMetadata Metadata { get; set; }
    }
}