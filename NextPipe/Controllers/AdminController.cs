using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NextPipe.Messaging.Infrastructure.Contracts;
using Serilog;

namespace NextPipe.Controllers
{
    [ApiController]
    [Route("core/addmin")]
    public class AdminController : BaseController
    {
        public AdminController(ILogger logger, IQueryRouter queryRouter, ICommandRouter commandRouter) : base(logger, queryRouter, commandRouter)
        {
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            return StatusCode(200);
        }


        [HttpGet]
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            return StatusCode(200);
        }
    }
}