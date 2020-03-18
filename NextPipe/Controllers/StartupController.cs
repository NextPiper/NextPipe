using NextPipe.Messaging.Infrastructure.Contracts;
using Serilog;

namespace NextPipe.Controllers
{
    public class StartupController : BaseController
    {
        public StartupController(ILogger logger, IQueryRouter queryRouter, ICommandRouter commandRouter) : base(logger, queryRouter, commandRouter)
        {
        }
    }
}