using System.Collections.Generic;
using System.Linq;
using NextPipe.Persistence.Entities.NextPipeModules;

namespace NextPipe.Core.Queries.Queries
{
    public class GetModulesPagedQuery : BaseQuery<IEnumerable<Module>>
    {
        public int Page { get; }
        
        public int PageSize { get; }

        public GetModulesPagedQuery(int page, int pageSize)
        {
            Page = page;
            PageSize = pageSize;
        }
    }
}