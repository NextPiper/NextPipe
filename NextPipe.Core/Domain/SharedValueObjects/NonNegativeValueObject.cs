using System;

namespace NextPipe.Core.Domain.SharedValueObjects
{
    public class NonNegativeValueObject<T> : BaseNumberValueObject<T> where T : struct
    {
        public NonNegativeValueObject(T value) : base(value)
        {
            if (IsNegative(value))
            {
                throw new ArgumentException($"Value {value} of type {nameof(NonNegativeValueObject<T>)} may not be negative");
            } 
        }

        private bool IsNegative(T value)
        {
            if (value is int)
            {
                var intV = Convert.ToInt32(value);
                if (intV < 0)
                {
                    return false;
                }
            }
            if (value is float)
            {
                var intV = Convert.ToSingle(value);
                if (intV < 0)
                {
                    return false;
                } 
            }
            if (value is double)
            {
                var intV = Convert.ToDouble(value);
                if (intV < 0)
                {
                    return false;
                } 
            }
            if (value is long)
            {
                var intV = Convert.ToInt64(value);
                if (intV < 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}