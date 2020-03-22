using System;
using NextPipe.Core.Domain.SharedValueObjects;

namespace NextPipe.Core.Domain.Module.ValueObjects
{
    public class ImageReplicas : NonNegativeValueObject<int>
    {
        public ImageReplicas(int value) : base(value)
        {
            if (value < 1)
            {
                throw new ArgumentException($"Value {value} in type {nameof(ImageReplicas)} may not be below 1");
            }
        }
    }
}