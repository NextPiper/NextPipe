using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NextPipe.Core.Commands.Commands;
using NextPipe.Core.Queries.Queries;
using NextPipe.Messaging.Infrastructure.Contracts;
using NextPipe.Utilities.Documents.Responses;
using Serilog;

namespace NextPipe.Controllers
{
    [ApiController]
    [Route("core/config")]
    public class ConfigController : BaseController
    {
        public ConfigController(ILogger logger, IQueryRouter queryRouter, ICommandRouter commandRouter) : base(logger, queryRouter, commandRouter)
        {
        }

        [HttpGet]
        [Route("mq")]
        public async Task<IActionResult> RequestMessageQue()
        {
            var cmdResult = await RouteAsync<TrialCommand, Response>(new TrialCommand());
            var queryResult = await QueryAsync<TrialQuery, string>(new TrialQuery());

            if (cmdResult.IsSuccessful)
            {
                return StatusCode(200);
            }
            else
            {
                return StatusCode(404);
            }
        }
    }
}