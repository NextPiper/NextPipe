using NextPipe.Core.Domain.SharedValueObjects;

namespace NextPipe.Core.Domain.Module.ValueObjects
{
    public class ModuleName : NonEmptyStringValueObject
    {
        public ModuleName(string value) : base(value)
        {
        }
    }
}