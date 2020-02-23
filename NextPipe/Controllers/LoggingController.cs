using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NextPipe.Controllers
{
    [ApiController]
    [Route("core/logging")]
    public class LoggingController
    {
        [HttpGet]
        [Route("log")]
        public async Task<IActionResult> RequestMessageQue(bool fatal)
        {
            return new ObjectResult("Logging fatal");
        }
    }
}