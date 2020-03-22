using System;
using System.Threading.Tasks;
using NextPipe.Persistence.Entities.NextPipeModules;

namespace NextPipe.Persistence.Repositories
{
    public interface IModuleRepository : IMongoRepository<Module>
    {
        Task<Module> GetModuleById(Guid id);
        Task UpdateModuleStatus(Guid id, ModuleStatus moduleStatus);
        Task AppendLog(Guid id, string log)
    }
    public class ModuleRepository: IMongoRepository<Module> 
    {
        
    }
}