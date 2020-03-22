using System;

namespace NextPipe.Core.Domain.SharedValueObjects
{
    public abstract class BaseNumberValueObject<T> : BaseValueObject<T> where T : struct
    {
        public BaseNumberValueObject(T value) : base(value)
        {
            if (!(value is int) && !(value is float) && !(value is double) && !(value is long))
            {
                throw new ArgumentException($"Value {value} in type {nameof(BaseNumberValueObject<T>)} must of either int, float, double or long");
            }
        }
    }
}