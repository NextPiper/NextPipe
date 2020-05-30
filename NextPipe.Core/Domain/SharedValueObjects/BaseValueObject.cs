using System.Collections.Generic;

namespace NextPipe.Core.Domain.SharedValueObjects
{
    public abstract class BaseValueObject<T> : ValueObject
    {
        public T Value { get; private set; }

        protected BaseValueObject(T value)
        {
            Value = value;
        }
        
        protected override IEnumerable<object> GetAtomicValues()
        {
            yield return Value;
        }
    }
}