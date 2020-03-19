using System;
using NextPipe.Core.Domain.SharedValueObjects;

namespace NextPipe.Core.ValueObjects
{
    public class ReplicaFailureThreshold : BaseValueObject<int>
    {
        public ReplicaFailureThreshold(int value) : base(value)
        {
            if (value < 4)
            {
                throw new ArgumentException($"Argument {nameof(value)} in type {nameof(ReplicaFailureThreshold)} may not be set to a value below 4");
            }
        }
    }
}