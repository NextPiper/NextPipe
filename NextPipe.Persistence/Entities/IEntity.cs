using System;

namespace NextPipe.Persistence.Entities
{
    public interface IEntity
    {
        Guid Id { get; }
    }
}