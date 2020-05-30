using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NextPipe.Messaging.Infrastructure.Contracts;
using Serilog;

namespace NextPipe.Controllers
{
    [ApiController]
    [Route("core/logging")]
    public class LoggingController : BaseController
    {
        public LoggingController(ILogger logger, IQueryRouter queryRouter, ICommandRouter commandRouter) : base(logger, queryRouter, commandRouter)
        {
        }

        [HttpGet]
        [Route("log")]
        public async Task<IActionResult> RequestMessageQue(bool fatal)
        {
            return new ObjectResult("Logging fatal");
        }
    }
}