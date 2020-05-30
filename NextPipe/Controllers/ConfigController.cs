using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NextPipe.ActionFilters;
using NextPipe.Core.Commands.Commands;
using NextPipe.Core.Queries.Models;
using NextPipe.Core.Queries.Queries;
using NextPipe.Messaging.Infrastructure.Contracts;
using NextPipe.Persistence.Configuration;
using NextPipe.Utilities.Documents.Responses;
using Serilog;

namespace NextPipe.Controllers
{
    [ApiController]
    [ServiceFilter(typeof(InfrastructureValidationFilter))]
    [Route("core/config")]
    public class ConfigController : BaseController
    {
        private readonly IOptions<MongoDBPersistenceConfiguration> _mongoConfig;

        public ConfigController(ILogger logger, IQueryRouter queryRouter, ICommandRouter commandRouter, IOptions<MongoDBPersistenceConfiguration> mongoConfig) : base(logger, queryRouter, commandRouter)
        {
            _mongoConfig = mongoConfig;
        }

        [HttpGet]
        [Route("rabbitmq")]
        public async Task<IActionResult> RequestMessageQue(bool loadBalancer = false)
        {
            var result =
                await QueryAsync<GetRabbitMQCredentialsQuery, RabbitMQConfig>(new GetRabbitMQCredentialsQuery(loadBalancer));

            return ReadDefaultQuery(result);
        }

        [HttpGet]
        [Route("mongoDB")]
        public async Task<IActionResult> RequestMongoDB()
        {
            return new ObjectResult(new { MongoClusterConnectionString = _mongoConfig.Value.MongoClusterConnectionString});
        }
    }
}