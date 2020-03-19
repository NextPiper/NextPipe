using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NextPipe.Persistence.Configuration;
using NextPipe.Persistence.Entities;

namespace NextPipe.Persistence.Repositories
{
    public interface ITasksRepository : IMongoRepository<NextPipeProcess>
    {
    }
    
    public class TasksRepository : BaseMongoRepository<NextPipeProcess>, ITasksRepository
    {
        public TasksRepository(IMongoClient mongoClient, IOptions<MongoDBPersistenceConfiguration> config) : base(mongoClient, config)
        {
        }
    }
}