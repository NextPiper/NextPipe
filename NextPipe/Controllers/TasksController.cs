using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NextPipe.Core.Commands.Commands.StartupCommands;
using NextPipe.Core.Domain.Module.ValueObjects;
using NextPipe.Core.Helpers;
using NextPipe.Core.Kubernetes;
using NextPipe.Core.Queries.Queries;
using NextPipe.Messaging.Infrastructure.Contracts;
using NextPipe.Persistence.Configuration;
using NextPipe.Persistence.Entities;
using NextPipe.Utilities.Documents.Responses;
using Microsoft.AspNetCore.Mvc;
using NextPipe.Core.ValueObjects;
using Serilog;

namespace NextPipe.Controllers
{
    [ApiController]
    [Route("core/tasks")]
    public class TasksController : BaseController
    {
        private readonly ILogHandler _logHandler;
        private readonly IKubectlHelper _kubectlHelper;

        public TasksController(ILogger logger, IQueryRouter queryRouter, ICommandRouter commandRouter, ILogHandler logHandler, IKubectlHelper kubectlHelper) : base(logger, queryRouter, commandRouter)
        {
            _logHandler = logHandler;
            _kubectlHelper = kubectlHelper;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("request-initialize-infrastructure")]
        public async Task<IActionResult> RequestInitializeInfrastructure()
        {
            var result = await RouteAsync<RequestInitializeInfrastructure, TaskRequestResponse>(new RequestInitializeInfrastructure());

            if (result.IsSuccessful)
            {
                return StatusCode(202, new {monitorUrl = $"core/tasks/{result.Id}", msg = result.Message});
            }

            return StatusCode(409, result.Message);
        }

        [HttpPost]
        [Route("request-uninstall-infrastructure")]
        public async Task<IActionResult> RequestUninstallInfrastructure()
        {
            return new ObjectResult("");
        }
        
        [HttpGet]
        [Route("{taskId}")]
        public async Task<IActionResult> GetTask(Guid taskId)
        {
            var result = await QueryAsync<GetTaskByIdQuery, NextPipeTask>(new GetTaskByIdQuery(taskId));

            return ReadDefaultQuery(result);
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetTasks(int page = 0, int pageSize = 100)
        {
           var result =
                await QueryAsync<GetTasksPagedQuery, IEnumerable<NextPipeTask>>(new GetTasksPagedQuery(page, pageSize));

            return ReadDefaultQuery(result);
        }
        
        [HttpPost]
        [Route("request-module-install")]
        public async Task<IActionResult> RequestInstallModule(string imagename, int amountofreplicas, string modulename)
        {
            return StatusCode(200); 
        }
    }
}