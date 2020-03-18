using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NextPipe.Configuration;
using NextPipe.Messaging.Infrastructure.Contracts;
using NextPipe.RequestModels;
using ILogger = Serilog.ILogger;

namespace NextPipe.Controllers
{
    [ApiController]
    [Route("core/health")]
    public class HealthController : BaseController
    {
        public HealthController(ILogger logger, IQueryRouter queryRouter, ICommandRouter commandRouter) : base(logger, queryRouter, commandRouter)
        {
        }

        [HttpGet]
        [Route("shallowHealth")]
        public async Task<IActionResult> ValidateShallowHealthStatus()
        {
            _logger.Information("Awesome information");
            return new ObjectResult("Up and running");
        }

        [HttpGet]
        [Route("deepHealthCheck")]
        public async Task<IActionResult> ValidateDeepHealthStatus([FromBody] InstallModuleRM moduleInstallRM)
        {
            return new ObjectResult("All services are up and running");
        }
    }
}
