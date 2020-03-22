using System;
using System.Text.RegularExpressions;
using NextPipe.Core.Domain.SharedValueObjects;

namespace NextPipe.Core.Domain.Module.ValueObjects
{
    public class ModuleName : NonEmptyStringValueObject
    {
        public ModuleName(string value) : base(value)
        {
            if (!IsValidModuleName(value))
            {
                throw new ArgumentException($"Value {value} in type {nameof(ModuleName)} has an invalid value: Must be a DNS-1123. a DNS-1123 subdomain must consist of lower case alphanumeric characters, '-' or '.', and must start and end with an alphanumeric character (e.g. 'example.com', regex used for validation is '[a-z0-9]([-a-z0-9]*[a-z0-9])?(\\.[a-z0-9]([-a-z0-9]*[a-z0-9])?)*')");
            }
        }

        private bool IsValidModuleName(string value)
        {
            Regex r = new Regex(@"^[a-z0-9]([-a-z0-9]*[a-z0-9])?(\.[a-z0-9]([-a-z0-9]*[a-z0-9])?)*$");

            return r.IsMatch(value);
        }
    }
}