using System;
using NextPipe.Core.Domain.SharedValueObjects;

namespace NextPipe.Core.ValueObjects
{
    public class LowerBoundaryReadyReplicas : BaseValueObject<int>
    {
        private int lowerBoundaryReadyReplicas = 2;
        
        public LowerBoundaryReadyReplicas(int value) : base(value)
        {
            if (value < lowerBoundaryReadyReplicas)
            {
                throw new ArgumentException($"{nameof(value)} in" +
                                            $" {nameof(LowerBoundaryReadyReplicas)} may not be below {lowerBoundaryReadyReplicas} value={value}");
            }
        }
    }
}