using System;
using NextPipe.Persistence.Entities.ProcessLock;

namespace NextPipe.Persistence.Entities.ArchivedObjects
{
    public class ArchiveObject : IEntity
    {
        public Guid Id { get; set;}
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime EditedAt { get; set; } = DateTime.Now;
        
        //possibly add time for when the task/module was "finished" and added to the archiverepository to get time stamp on whole lifecycle.
        public NextPipeObjectType Type { get; set; }
        public Guid TypeReferenceId { get; set; }
        public ReasonForArchive ArchiveReason {get; set; }
        
        //possibly also add metadata that could have hostname, modulename, modulereplica 
        public BaseArchivable Metadata { get; set; }
    }
}