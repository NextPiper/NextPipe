using System;
using NextPipe.Core.Domain.SharedValueObjects;

namespace NextPipe.Core.ValueObjects
{
    public class ReplicaDelaySeconds : BaseValueObject<int>
    {
        public ReplicaDelaySeconds(int value) : base(value)
        {
            if (value < 30)
            {
                throw new ArgumentException($"Argument {nameof(value)} in type {nameof(ReplicaDelaySeconds)} may not be set to a value below 30 secs");
            }
        }
    }
}