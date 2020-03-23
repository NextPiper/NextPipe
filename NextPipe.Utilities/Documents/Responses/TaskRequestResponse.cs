using System;

namespace NextPipe.Utilities.Documents.Responses
{
    public class TaskRequestResponse : IdResponse
    {
        public string Message { get; }

        public TaskRequestResponse(Guid id, string message, bool isSuccessful = true, Exception exception = null) : base(id, isSuccessful, exception)
        {
            Message = message;
        }

        public static TaskRequestResponse AttachToRunningProcess(Guid taskId, string msg)
        {
            return new TaskRequestResponse(taskId, msg);
        }

        public static TaskRequestResponse InfrastructureAlreadyRunning(string msg)
        {
            return new TaskRequestResponse(Guid.Empty, msg, false);
        }

        public static TaskRequestResponse TaskRequestAccepted(Guid taskId, string msg)
        {
            return new TaskRequestResponse(taskId, msg);
        }
    }
}