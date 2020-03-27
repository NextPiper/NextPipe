using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NextPipe.Core.Commands.Commands.ModuleCommands;
using NextPipe.Core.Domain.Module.ValueObjects;
using NextPipe.Core.Kubernetes;
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
        private readonly IKubectlHelper _kubectlHelper;

        public ModuleController(ILogger logger, IQueryRouter queryRouter, ICommandRouter commandRouter,IKubectlHelper kubectlHelper) : base(logger, queryRouter, commandRouter)
        {
            _kubectlHelper = kubectlHelper;
        }
        
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> InstallModule(string imageName, int amountOfReplicas, string moduleName)
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

        [HttpPut]
        [Route("{id}")]
        public async Task<IActionResult> ScaleModule(Guid id, int replicas)
        {
            var result = await RouteAsync<ScaleModuleCommand, Response>(new ScaleModuleCommand(id, replicas));

            return ReadDefaultResponse(result, failureCode: 400);
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteModule(Guid id)
        {
            var result = await RouteAsync<RequestDeleteModuleCommand, Response>(new RequestDeleteModuleCommand(id));

            return ReadDefaultResponse(result, failureCode:400);
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

        [HttpPost]
        [Route("trial")]
        public async Task<IActionResult> Trial()
        {
            await _kubectlHelper.InstallModule(KubectlHelper.CreateModuleDeployment(
                "nginx",
                "nginx",
                3));


            return StatusCode(200);
        }
    }
}