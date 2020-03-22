namespace NextPipe.Core.Domain.SharedValueObjects
{
    public class NonNegativeValueObject<T> : BaseValueObject<T> where T : struct, new()
    {
        public NonNegativeValueObject(T value) : base(value)
        {
        }
    }
}