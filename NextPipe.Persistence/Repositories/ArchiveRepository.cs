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
        public Task UpdateReasonForArchive(Guid id, ReasonForArchive reason);
        public Task UpdateNextPipeObjectType(Guid id, NextPipeObjectType nextPipeObjectType);

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
    }
}