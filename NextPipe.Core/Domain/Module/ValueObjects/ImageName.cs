using NextPipe.Core.Domain.SharedValueObjects;

namespace NextPipe.Core.Domain.Module.ValueObjects
{
    public class ImageName : NonNullValueObject<string>
    {
        public ImageName(string value) : base(value)
        {
        }
    }
}