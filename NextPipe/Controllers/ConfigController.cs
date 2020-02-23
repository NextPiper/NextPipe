using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace NextPipe.Controllers
{
    [ApiController]
    [Route("core/config")]
    public class ConfigController
    {
        [HttpGet]
        [Route("mq")]
        public async Task<IActionResult> RequestMessageQue()
        {
            return new ObjectResult("127.0.0.1/mq-maguns");
        }
    }
}