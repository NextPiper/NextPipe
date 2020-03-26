using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NextPipe.Core.Commands.Commands.ModuleCommands;
using NextPipe.Core.Queries.Queries;
using NextPipe.Messaging.Infrastructure.Contracts;
using NextPipe.Persistence.Entities.NextPipeModules;
using NextPipe.Utilities.Documents.Responses;
using Serilog;

namespace NextPipe.Controllers
{
    [ApiController]
    [Route("core/modules")]
    public class ModuleController : BaseController
    {
        public ModuleController(ILogger logger, IQueryRouter queryRouter, ICommandRouter commandRouter) : base(logger, queryRouter, commandRouter)
        {
        }
        
        [HttpPost]
        [Route("")]
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

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetModules(int page, int pageSize)
        {
            var result = await QueryAsync<GetModulesPagedQuery, IEnumerable<Module>>
                (new GetModulesPagedQuery(page, pageSize));

            return ReadDefaultQuery(result);
        }

        [HttpGet]
        [Route("{moduleId}")]
        public async Task<IActionResult> GetModule(Guid moduleId)
        {
            var result = await QueryAsync<GetModuleByIdQuery, Module>(new GetModuleByIdQuery(moduleId));

            return ReadDefaultQuery(result);
        }
        
        
    }
}