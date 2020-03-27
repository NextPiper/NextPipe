using System;
using NextPipe.Core.Domain.SharedValueObjects;

namespace NextPipe.Core.Domain.Module.ValueObjects
{
    public class ModuleReplicas : NonNegativeValueObject<int>
    {
        public ModuleReplicas(int value) : base(value)
        {
            if (value < 1)
            {
                throw new ArgumentException($"Value {value} in type {nameof(ModuleReplicas)} may not be below 1");
            }
        }
    }
}