using System;

namespace NextPipe.Persistence.Entities
{
    public class NextPipeTask : IEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime EditedAt { get; set; }

        // The specific TaskId for cross reference
        public Guid TaskId { get; set; }
        public QueueStatus QueueStatus { get; set; }
        public TaskStatus TaskStatus { get; set; }
        public TaskType TaskType { get; set; }
        
        public TaskPriority TaskPriority { get; set; }
        
        public string Logs { get; set; }
    }
}