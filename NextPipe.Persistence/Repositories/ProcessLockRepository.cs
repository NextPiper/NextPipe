using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NextPipe.Persistence.Configuration;
using NextPipe.Persistence.Entities.ProcessLock;

namespace NextPipe.Persistence.Repositories
{
    public interface IProcessLockRepository : IMongoRepository<ProcessLock>
    {
        Task<ProcessLock> FindProcessLockByProcessType(NextPipeProcessType type);
        Task<ProcessLock> ReplaceProcessLock(ProcessLock processLock, ProcessLock replaceLock);
        Task<ProcessLock> InsertAndReturn(ProcessLock processLock, string hostname = null);
    }
    
    public class ProcessLockRepository : BaseMongoRepository<ProcessLock>, IProcessLockRepository
    {
        public ProcessLockRepository(IMongoClient mongoClient, IOptions<MongoDBPersistenceConfiguration> config) : base(mongoClient, config)
        {
            // Create a unique index on the process type, to ensure that there can never be more than one host running a
            // specific process.
            var options = new CreateIndexOptions { Unique = true };
            var field = new StringFieldDefinition<ProcessLock>("NextPipeProcessType");
            var indexDefinition = new IndexKeysDefinitionBuilder<ProcessLock>().Ascending(field);
            var indexModel = new CreateIndexModel<ProcessLock>(indexDefinition,options);
            Database.GetCollection<ProcessLock>(CollectionName).Indexes.CreateOne(indexModel);      
        }

        public async Task<ProcessLock> FindProcessLockByProcessType(NextPipeProcessType type)
        {
            return await Collection().Find(t => t.NextPipeProcessType == type.ToString()).SingleOrDefaultAsync();
        }

        /// <summary>
        /// Replace a processLock, it will not update if lock to be replaced is not present. This means that another
        /// replicas has already replaced the lock thus we were beat and should not do anything
        /// </summary>
        /// <param name="processLock"></param>
        /// <param name="replaceLock"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ProcessLock> ReplaceProcessLock(ProcessLock processLock, ProcessLock replaceLock)
        {
            try
            {
                var result = await Collection().FindOneAndUpdateAsync(
                    t => t.ProcessId == replaceLock.ProcessId,
                    Update
                        .Set(t => t.ProcessId, processLock.ProcessId)
                        .Set(t => t.Hostname, processLock.Hostname)
                        .Set(t => t.EditedAt, DateTime.Now));
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        /// <summary>
        /// Returns null if insert failed
        /// </summary>
        /// <param name="processLock"></param>
        /// <returns></returns>
        public async Task<ProcessLock> InsertAndReturn(ProcessLock processLock, string hostname = null)
        {
            try
            {
                await Collection().InsertOneAsync(processLock, new InsertOneOptions());
                return processLock;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception Thrown while trying too insert processLock of type : {processLock.NextPipeProcessType} for host: {hostname} --> {e.Message}");
                return null;
            }
        }
    }
}