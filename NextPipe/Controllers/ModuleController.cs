using Microsoft.AspNetCore.Mvc;
using NextPipe.Messaging.Infrastructure.Contracts;
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
    }
}