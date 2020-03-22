namespace NextPipe.Core.Domain.SharedValueObjects
{
    public class BaseNumberValueObject<T> : BaseValueObject<T> where T : struct
    {
        public BaseNumberValueObject(T value) : base(value)
        {
        }
    }
}