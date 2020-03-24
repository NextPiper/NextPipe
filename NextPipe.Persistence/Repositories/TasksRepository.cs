using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LamarCodeGeneration.Frames;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NextPipe.Persistence.Configuration;
using NextPipe.Persistence.Entities;
using NextPipe.Persistence.Exceptions;
using TaskStatus = NextPipe.Persistence.Entities.TaskStatus;

namespace NextPipe.Persistence.Repositories
{
    public interface ITasksRepository : IMongoRepository<NextPipeTask>
    {
        Task<IEnumerable<NextPipeTask>> GetTasksByTaskType(TaskType taskType);
        Task<NextPipeTask> GetTaskByTaskId(Guid taskId);
        Task UpdateTaskQueueStatus(Guid taskId, QueueStatus queueStatus);
        Task UpdateTaskStatus(Guid taskId, TaskStatus taskStatus);
        Task<NextPipeTask> SetTaskStartedWithMetadata<T>(Guid taskId, QueueStatus queueStatus, TaskStatus taskStatus, T metadata) where T : BaseMetadata;
        Task<NextPipeTask> SetTaskStarted(Guid taskId, QueueStatus queueStatus, TaskStatus taskStatus);
        Task AppendLog(Guid taskId, string message);
        Task FinishTask(Guid taskId, TaskStatus taskStatus, string writeLogs = null);
        Task<IEnumerable<NextPipeTask>> GetAllRunningTasks(int page, int pageSize);
        Task IncrementRestarts(Guid taskId, string hostname, string logs = null);
        Task UpdateStatus(Guid taskId, TaskStatus taskStatus, QueueStatus queueStatus);
    }
    
    public class TasksRepository : BaseMongoRepository<NextPipeTask>, ITasksRepository
    {
        public TasksRepository(IMongoClient mongoClient, IOptions<MongoDBPersistenceConfiguration> config) : base(mongoClient, config)
        { 
        }

        public override async Task<Guid> Insert(NextPipeTask entity)
        {
            if (entity.TaskType == TaskType.RabbitInfrastructureDeploy)
            {
                var result = await GetTasksByTaskType(TaskType.RabbitInfrastructureDeploy);
                if (result.Any())
                {
                    foreach (var nextPipeTask in result)
                    {
                        if (nextPipeTask.QueueStatus != QueueStatus.Completed)
                        {
                            throw new PersistenceException($"Can't Que multiple {nameof(NextPipeTask)} of TaskType {nameof(TaskType.RabbitInfrastructureDeploy)}");
                        }
                    }
                } 
            }

            if (entity.TaskType == TaskType.RabbitInfrastructureUninstall)
            {
                var result = await GetTasksByTaskType(TaskType.RabbitInfrastructureUninstall);
                if (result.Any())
                {
                    foreach (var nextPipeTask in result)
                    {
                        if (nextPipeTask.QueueStatus != QueueStatus.Completed)
                        {
                            throw new PersistenceException($"Can't Que multiple {nameof(NextPipeTask)} of TaskType {nameof(TaskType.RabbitInfrastructureDeploy)}");
                        }
                    }
                }
            }
            
            await Collection().InsertOneAsync(entity, new InsertOneOptions());

            return entity.Id;
        }

        public async Task<IEnumerable<NextPipeTask>> GetTasksByTaskType(TaskType taskType)
        {
            return await Collection().Find(item => item.TaskType == taskType).ToListAsync();
        }

        public async Task<NextPipeTask> GetTaskByTaskId(Guid taskId)
        {
            return await Collection().Find(item => item.TaskId == taskId).SingleOrDefaultAsync();
        }

        public async Task UpdateTaskQueueStatus(Guid taskId, QueueStatus queueStatus)
        {
            await Collection()
                .FindOneAndUpdateAsync(t => t.TaskId == taskId, Update.Set(t => t.QueueStatus, queueStatus));
        }

        public async Task UpdateTaskStatus(Guid taskId, TaskStatus taskStatus)
        {
            await Collection().FindOneAndUpdateAsync(t => t.TaskId == taskId, Update.Set(t => t.TaskStatus, taskStatus));
        }

        public async Task<NextPipeTask> SetTaskStartedWithMetadata<T>(Guid taskId, QueueStatus queueStatus, TaskStatus taskStatus, T metadata) where T : BaseMetadata
        {
            return await Collection().FindOneAndUpdateAsync(t => t.TaskId == taskId, Update
                .Set(t => t.QueueStatus, queueStatus)
                .Set(t => t.TaskStatus, taskStatus)
                .Set(t => t.Metadata, metadata));
        }

        public async Task<NextPipeTask> SetTaskStarted(Guid taskId, QueueStatus queueStatus, TaskStatus taskStatus)
        {
            return await Collection().FindOneAndUpdateAsync(t => t.TaskId == taskId, Update
                .Set(t => t.QueueStatus, queueStatus)
                .Set(t => t.TaskStatus, taskStatus));
        }

        public async Task AppendLog(Guid taskId, string message)
        {
            //await Collection().Find(t => t.TaskId == taskId).ForEachAsync((e, i) => { e.Logs += message; });
            await Collection().FindOneAndUpdateAsync(t => t.TaskId == taskId, Update
                .Set(t => t.Logs, message));
        }

        public async Task FinishTask(Guid taskId, TaskStatus taskStatus, string writeLogs = null)
        {
            if (writeLogs != null)
            {
                await Collection().FindOneAndUpdateAsync(t => t.TaskId == taskId, Update
                    .Set(t => t.TaskStatus, taskStatus)
                    .Set(t => t.QueueStatus, QueueStatus.Completed)
                    .Set(t => t.Logs, writeLogs));
            }
            else
            {
                await Collection().FindOneAndUpdateAsync(t => t.TaskId == taskId, Update
                    .Set(t => t.TaskStatus, taskStatus)
                    .Set(t => t.QueueStatus, QueueStatus.Completed));
            }
        }

        public async Task<IEnumerable<NextPipeTask>> GetAllRunningTasks(int page, int pageSize)
        {
            return await Collection()
                .Find(t => t.TaskStatus == TaskStatus.Running)
                .Skip(page * pageSize)
                .Limit(pageSize)
                .ToListAsync();
        }

        public async Task IncrementRestarts(Guid taskId, string hostname, string logs = null)
        {
            if (logs == null)
            {
                await Collection().FindOneAndUpdateAsync(t => t.TaskId == taskId, 
                    Update
                        .Inc(t => t.Restarts, 1)
                        .Set(t => t.Hostname, hostname));   
            }
            else
            {
                await Collection().FindOneAndUpdateAsync(t => t.TaskId == taskId, 
                    Update
                        .Inc(t => t.Restarts, 1)
                        .Set(t => t.Logs, logs)
                        .Set(t => t.Hostname, hostname));
            }
        }

        public async Task UpdateStatus(Guid taskId, TaskStatus taskStatus, QueueStatus queueStatus)
        {
            await Collection().FindOneAndUpdateAsync(t => t.TaskId == taskId, 
                Update
                .Set(t => t.QueueStatus, queueStatus)
                .Set(t => t.TaskStatus, taskStatus));
        }
    }
}