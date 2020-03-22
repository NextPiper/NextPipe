using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper.Configuration.Annotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NextPipe.Core.Commands.Commands.ModuleCommands;
using NextPipe.Core.Commands.Commands.StartupCommands;
using NextPipe.Core.Helpers;
using NextPipe.Core.Kubernetes;
using NextPipe.Core.Queries.Queries;
using NextPipe.Messaging.Infrastructure.Contracts;
using NextPipe.Persistence.Configuration;
using NextPipe.Persistence.Entities;
using NextPipe.Utilities.Core;
using NextPipe.Utilities.Documents.Responses;
using Serilog;

namespace NextPipe.Controllers
{
    [ApiController]
    [Route("core/tasks")]
    public class TasksController : BaseController
    {
        public IOptions<MongoDBPersistenceConfiguration> Conf { get; }
        public ILogHandler Handler { get; }
        private readonly IKubectlHelper _kubectlHelper;

        public TasksController(ILogger logger, IQueryRouter queryRouter, ICommandRouter commandRouter, IKubectlHelper kubectlHelper, IOptions<MongoDBPersistenceConfiguration> conf, ILogHandler handler) : base(logger, queryRouter, commandRouter)
        {
            Conf = conf;
            Handler = handler;
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
           /*var result =
                await QueryAsync<GetTasksPagedQuery, IEnumerable<NextPipeTask>>(new GetTasksPagedQuery(page, pageSize));*/

           Handler.WriteLine("Hello");
           Handler.WriteLine("Heya");
           Console.WriteLine(Handler.GetLastUpdate());
           Handler.WriteLine("Yup");
           Console.WriteLine(Handler.GetLastWrite());
            
            
            
            return ReadDefaultQuery("hello");
        }
        
        [HttpPost]
        [Route("request-module-install")]
        public async Task<IActionResult> RequestInstallModule(string imageName, int amountOfReplicas, string moduleName)
        {
            var result =
                await RouteAsync<RequestInstallModule, TaskRequestResponse>(
                    new RequestInstallModule(imageName, amountOfReplicas, moduleName));
            
            if (result.IsSuccessful)
            {
                return StatusCode(202, new {monitorUrl = $"core/tasks/{result.Id}", msg = result.Message});
            }
            
            return StatusCode(409, result.Message); 
        }

    }
}