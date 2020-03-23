using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NextPipe.Persistence.Configuration;
using NextPipe.Persistence.Entities;
using NextPipe.Persistence.Entities.NextPipeModules;

namespace NextPipe.Persistence.Repositories
{
    public interface IModuleRepository : IMongoRepository<Module>
    {
        Task UpdateModuleStatus(Guid id, ModuleStatus moduleStatus);
        Task<Module> GetModuleByImageName(string imageName);
        Task AppendLog(Guid id, string log);
        Task<Module> GetModuleByModuleName(string moduleName);
    }

    public class ModuleRepository : BaseMongoRepository<Module>, IModuleRepository
    {

        public ModuleRepository(IMongoClient mongoClient, IOptions<MongoDBPersistenceConfiguration> config) : base(
            mongoClient, config)
        {
            var options = new CreateIndexOptions { Unique = true };
            var field = new StringFieldDefinition<Module>("ImageName");
            var indexDefinition = new IndexKeysDefinitionBuilder<Module>().Ascending(field);
            var indexModel = new CreateIndexModel<Module>(indexDefinition,options);
            Database.GetCollection<Module>(CollectionName).Indexes.CreateOne(indexModel);
            
            var options2 = new CreateIndexOptions { Unique = true };
            var field2 = new StringFieldDefinition<Module>("ModuleName");
            var indexDefinition2 = new IndexKeysDefinitionBuilder<Module>().Ascending(field);
            var indexModel2 = new CreateIndexModel<Module>(indexDefinition,options);
            Database.GetCollection<Module>(CollectionName).Indexes.CreateOne(indexModel);
            
        }

        public async Task UpdateModuleStatus(Guid id, ModuleStatus moduleStatus)
        {
            await Collection().FindOneAndUpdateAsync(t => t.Id == id, Update.Set(t => t.ModuleStatus, moduleStatus));
        }

        public async Task<Module> GetModuleByImageName(string imageName)
        {
            return await Collection().Find(item => item.ImageName.Equals(imageName)).SingleOrDefaultAsync();

        }

        public async Task AppendLog(Guid id, string log)
        {
            await Collection().FindOneAndUpdateAsync(t => t.Id == id, Update
                .Set(t => t.Logs, log));
        }

        public async Task<Module> GetModuleByModuleName(string moduleName)
        {
            return await Collection().Find(t => t.ModuleName.Equals(moduleName)).SingleOrDefaultAsync();
        }
    }
}