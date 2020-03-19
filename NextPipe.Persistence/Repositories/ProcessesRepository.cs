using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NextPipe.Persistence.Configuration;
using NextPipe.Persistence.Entities;

namespace NextPipe.Persistence.Repositories
{
    public interface IProcessesRepository : IMongoRepository<NextPipeProcess>
    {
    }
    
    public class ProcessesRepository : BaseMongoRepository<NextPipeProcess>, IProcessesRepository
    {
        public ProcessesRepository(IMongoClient mongoClient, IOptions<MongoDBPersistenceConfiguration> config) : base(mongoClient, config)
        {
        }
    }
}