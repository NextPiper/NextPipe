using System;

namespace NextPipe.Utilities.Documents.Responses
{
    public class IdResponse : Response
    {
        public IFormattable Id { get; private set; }

        public IdResponse(IFormattable id, bool isSuccessful = true, Exception exception = null) : base(isSuccessful, exception)
        {
            Id = id;
        }
    }
}