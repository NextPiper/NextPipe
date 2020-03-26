using System;
using NextPipe.Persistence.Entities.NextPipeModules;

namespace NextPipe.Core.Queries.Queries
{
    public class GetModuleByIdQuery : BaseQuery<Module>
    {
        public Guid ModuleId { get; }

        public GetModuleByIdQuery(Guid moduleId)
        {
            ModuleId = moduleId;
        }
    }
}