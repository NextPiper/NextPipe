using System;
using System.Linq;

namespace NextPipe.Core.Domain.SharedValueObjects
{
    public class NonEmptyStringValueObject : NonNullValueObject<string>
    {
        public NonEmptyStringValueObject(string value) : base(value)
        {
            if (!value.Any())
            {
                throw new ArgumentException($"Value {value} of Type {nameof(NonEmptyStringValueObject)} may not me empty");
            }
        }
    }
}