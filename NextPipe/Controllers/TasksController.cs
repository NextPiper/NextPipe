using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper.Configuration.Annotations;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using NextPipe.Core.Commands.Commands.ModuleCommands;
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
using NextPipe.Core.Commands.Commands.ProcessLockCommands;
using NextPipe.Core.Domain.NextPipeTask.ValueObject;
using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Core.ValueObjects;
using NextPipe.Persistence.Repositories;
using Serilog;
using TaskStatus = NextPipe.Persistence.Entities.TaskStatus;

namespace NextPipe.Controllers
{
    [ApiController]
    [Microsoft.AspNetCore.Mvc.Route("core/tasks")]
    public class TasksController : BaseController
    {
        public TasksController(ILogger logger, IQueryRouter queryRouter, ICommandRouter commandRouter) : base(logger, queryRouter, commandRouter)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("request-initialize-infrastructure")]
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
        [Microsoft.AspNetCore.Mvc.Route("request-uninstall-infrastructure")]
        public async Task<IActionResult> RequestUninstallInfrastructure()
        {
            var result =
                await RouteAsync<RequestUninstallInfrastructure, TaskRequestResponse>(
                    new RequestUninstallInfrastructure());
            
            if (result.IsSuccessful)
            {
                return StatusCode(202, new {monitorUrl = $"core/tasks/{result.Id}", msg = result.Message});
            }

            return StatusCode(409, result.Message);
        }
        
        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("{taskId}")]
        public async Task<IActionResult> GetTask(Guid taskId)
        {
            var result = await QueryAsync<GetTaskByIdQuery, NextPipeTask>(new GetTaskByIdQuery(taskId));

            return ReadDefaultQuery(result);
        }

        [HttpGet]
        [Microsoft.AspNetCore.Mvc.Route("")]
        public async Task<IActionResult> GetTasks(int page = 0, int pageSize = 100)
        {
           var result =
                await QueryAsync<GetTasksPagedQuery, IEnumerable<NextPipeTask>>(new GetTasksPagedQuery(page, pageSize));

            return ReadDefaultQuery(result);
        }
        
        [HttpPost]
        [Microsoft.AspNetCore.Mvc.Route("request-module-install")]
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
