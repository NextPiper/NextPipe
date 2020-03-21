using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NextPipe.Core.Queries.Queries;
using NextPipe.Persistence.Entities;
using NextPipe.Persistence.Repositories;
using SimpleSoft.Mediator;

namespace NextPipe.Core.Queries.Handlers
{
    public class TasksQueryHandler : 
        IQueryHandler<GetTaskByIdQuery, NextPipeTask>,
        IQueryHandler<GetTasksPagedQuery, IEnumerable<NextPipeTask>>
    {
        private readonly ITasksRepository _tasksRepository;

        public TasksQueryHandler(ITasksRepository tasksRepository)
        {
            _tasksRepository = tasksRepository;
        }


        public async Task<NextPipeTask> HandleAsync(GetTaskByIdQuery query, CancellationToken ct)
        {
            return await _tasksRepository.GetTaskByTaskId(query.TaskId);
        }

        public async Task<IEnumerable<NextPipeTask>> HandleAsync(GetTasksPagedQuery query, CancellationToken ct)
        {
            return await _tasksRepository.GetPaged(query.Page, query.PageSize);
        }
    }
}