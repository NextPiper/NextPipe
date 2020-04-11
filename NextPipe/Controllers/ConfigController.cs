using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NextPipe.ActionFilters;
using NextPipe.Core.Commands.Commands;
using NextPipe.Core.Queries.Models;
using NextPipe.Core.Queries.Queries;
using NextPipe.Messaging.Infrastructure.Contracts;
using NextPipe.Utilities.Documents.Responses;
using Serilog;

namespace NextPipe.Controllers
{
    [ApiController]
    [ServiceFilter(typeof(InfrastructureValidationFilter))]
    [Route("core/config")]
    public class ConfigController : BaseController
    {
        public ConfigController(ILogger logger, IQueryRouter queryRouter, ICommandRouter commandRouter) : base(logger, queryRouter, commandRouter)
        {
        }

        [HttpGet]
        [Route("rabbitmq")]
        public async Task<IActionResult> RequestMessageQue(bool loadBalancer = false)
        {
            var result =
                await QueryAsync<GetRabbitMQCredentialsQuery, RabbitMQConfig>(new GetRabbitMQCredentialsQuery(loadBalancer));

            return ReadDefaultQuery(result);
        }
    }
}