using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NextPipe.Persistence.Configuration;
using NextPipe.Persistence.Entities.ArchivedObjects;

namespace NextPipe.Persistence.Repositories
{
    public interface IArchiveRepository : IMongoRepository<ArchiveObject>
    {
        Task UpdateReasonForArchive(Guid id, ReasonForArchive reason);
        Task UpdateNextPipeObjectType(Guid id, NextPipeObjectType nextPipeObjectType);
        Task<ArchiveObject> GetArchiveByTypeAndReferenceId(Guid id, NextPipeObjectType type);

    }
    public class ArchivedRepository : BaseMongoRepository<ArchiveObject>, IArchiveRepository
    {
        public ArchivedRepository(IMongoClient mongoClient, IOptions<MongoDBPersistenceConfiguration> config) : base(mongoClient, config)
        {
        }

        public Task UpdateReasonForArchive(Guid id, ReasonForArchive reason)
        {
            throw new NotImplementedException();
        }

        public Task UpdateNextPipeObjectType(Guid id, NextPipeObjectType nextPipeObjectType)
        {
            throw new NotImplementedException();
        }

        public async Task<ArchiveObject> GetArchiveByTypeAndReferenceId(Guid id, NextPipeObjectType type)
        {
            return await Collection().Find(t => t.Type == type && t.TypeReferenceId == id).SingleOrDefaultAsync();
        }
    }
}