using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using NextPipe.Core.Domain.SharedValueObjects;

namespace NextPipe.Core.Domain.Module.ValueObjects
{
    public class ImageName : NonNullValueObject<string>
    {
        private const string INVALID_SUBSTRING = ":latests";
        public ImageName(string value) : base(value)
        {
            if (!value.Any() || value.Contains(INVALID_SUBSTRING))
            {
                throw new ArgumentException($"Value {value} in type {typeof(ImageName)} may not be empty or contain {INVALID_SUBSTRING}");
            }
        }
    }
}