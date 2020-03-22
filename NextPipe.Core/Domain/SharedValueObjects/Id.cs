using System;

namespace NextPipe.Core.Domain.SharedValueObjects
{
    public class Id : BaseValueObject<Guid>
    {
        public Id() : base(Guid.NewGuid())
        {
        }

        public Id(Guid value) : base(value)
        {
            if (value == Guid.Empty || value == null)
            {
                throw new ArgumentException($"typeOf {nameof(Id)} can't be instantiated with type Guid.Empty or null");
            }
        }
    }
}