using System;

namespace NextPipe.Utilities.Documents.Responses
{
    public class IdResponse : Response
    {
        public Guid Id { get; private set; }

        public IdResponse(Guid id, bool isSuccessful = true, Exception exception = null) : base(isSuccessful, exception)
        {
            Id = id;
        }
    }
}