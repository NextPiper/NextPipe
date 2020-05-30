using System;
using NextPipe.Persistence.Entities;

namespace NextPipe.Core.Queries.Queries
{
    public class GetTaskByIdQuery : BaseQuery<NextPipeTask>
    {
        public Guid TaskId { get; }

        public GetTaskByIdQuery(Guid taskId)
        {
            TaskId = taskId;
        }
    }
}