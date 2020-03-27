using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NextPipe.Core.Queries.Queries;
using NextPipe.Persistence.Entities.NextPipeModules;
using NextPipe.Persistence.Repositories;
using SimpleSoft.Mediator;

namespace NextPipe.Core.Queries.Handlers
{
    public class ModuleQueryHandler: 
        IQueryHandler<GetModulesPagedQuery,IEnumerable<Module>>, 
        IQueryHandler<GetModuleByIdQuery, Module>
    {
        private readonly IModuleRepository _moduleRepository;
        
        public ModuleQueryHandler(IModuleRepository moduleRepository)
        {
            _moduleRepository = moduleRepository;
        }

        public async Task<IEnumerable<Module>> HandleAsync(GetModulesPagedQuery query, CancellationToken ct)
        {
            return await _moduleRepository.GetPaged(query.Page, query.PageSize);
        }

        public async Task<Module> HandleAsync(GetModuleByIdQuery query, CancellationToken ct)
        {
            return await _moduleRepository.GetById(query.ModuleId);
        }
    }
}