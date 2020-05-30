using System.Collections.Generic;
using NextPipe.Persistence.Entities;

namespace NextPipe.Core.Queries.Queries
{
    public class GetTasksPagedQuery : BaseQuery<IEnumerable<NextPipeTask>>
    {
        public int Page { get; }
        public int PageSize { get; }

        public GetTasksPagedQuery(int page, int pageSize)
        {
            Page = page;
            PageSize = pageSize;
        }
    }
}