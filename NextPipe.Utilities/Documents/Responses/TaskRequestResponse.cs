using System;

namespace NextPipe.Utilities.Documents.Responses
{
    public class TaskRequestResponse : IdResponse
    {
        public TaskRequestResponse(Guid id, bool isSuccessful = true, Exception exception = null) : base(id, isSuccessful, exception)
        {
        }
    }
}