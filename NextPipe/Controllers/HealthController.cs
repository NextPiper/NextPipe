using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NextPipe.Configuration;
using ILogger = Serilog.ILogger;

namespace NextPipe.Controllers
{
    [ApiController]
    [Route("core/health")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger _logger;

        public HealthController(ILogger logger)
        {
            _logger = logger;
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
        public async Task<IActionResult> ValidateDeepHealthStatus()
        {
            return new ObjectResult("All services are up and running");
        }
    }
}
