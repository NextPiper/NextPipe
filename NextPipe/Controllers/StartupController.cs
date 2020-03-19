using System.Threading.Tasks;
using AutoMapper.Configuration.Annotations;
using Microsoft.AspNetCore.Mvc;
using NextPipe.Core.Commands.Commands.StartupCommands;
using NextPipe.Messaging.Infrastructure.Contracts;
using NextPipe.Utilities.Documents.Responses;
using Serilog;

namespace NextPipe.Controllers
{
    [ApiController]
    [Route("core/startup")]
    public class StartupController : BaseController
    {
        public StartupController(ILogger logger, IQueryRouter queryRouter, ICommandRouter commandRouter) : base(logger, queryRouter, commandRouter)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("/requests")]
        public async Task<IActionResult> RequestInitializeInfrastructure()
        {
            var result = await RouteAsync<RequestInitializeInfrastructure, Response>(new RequestInitializeInfrastructure());

            return ReadDefaultResponse(result);
        }
    }
}