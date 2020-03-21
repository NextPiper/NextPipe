using System;

namespace NextPipe.Persistence.Entities
{
    public interface IEntity
    {
        Guid Id { get; }
        DateTime CreatedAt { get; set; }
        DateTime EditedAt { get; set; }
    }
}