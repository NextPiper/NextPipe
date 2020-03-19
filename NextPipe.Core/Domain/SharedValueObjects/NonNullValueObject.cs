using System;

namespace NextPipe.Core.Domain.SharedValueObjects
{
    public class NonNullValueObject<T> : BaseValueObject<T>
    {
        public NonNullValueObject(T value) : base(value)
        {
            if (value == null)
            {
                throw new ArgumentException($"Value: {nameof(value)} typeOf({typeof(T)}) in {nameof(NonNullValueObject<T>)} may not be null");   
            }
        }
    }
}