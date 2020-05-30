using System;
using NextPipe.Core.Domain.SharedValueObjects;

namespace NextPipe.Core.ValueObjects
{
    public class RabbitNumberOfReplicas : NonNegativeValueObject<int>
    {
        private int lowestNumberRabbitReplicas = 3;
        
        public RabbitNumberOfReplicas(int value) : base(value)
        {
            if (value < lowestNumberRabbitReplicas)
            {
                throw new ArgumentException($"Value {value} in type {nameof(RabbitNumberOfReplicas)} can't be below {lowestNumberRabbitReplicas}");
            }
        }
    }
}