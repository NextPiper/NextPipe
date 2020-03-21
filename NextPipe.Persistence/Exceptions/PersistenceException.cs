using System;

namespace NextPipe.Persistence.Exceptions
{
    public class PersistenceException : Exception
    {
        public PersistenceException(string message) : base(message)
        {
            Console.WriteLine(message);
        }
    }
}