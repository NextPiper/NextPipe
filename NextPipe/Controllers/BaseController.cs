using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NextPipe.Messaging.Infrastructure.Contracts;
using NextPipe.Utilities.Documents.Responses;
using Serilog;
using SimpleSoft.Mediator;

namespace NextPipe.Controllers
{
    public class BaseController : ControllerBase
    {
        protected readonly ILogger _logger;
        private readonly IQueryRouter _queryRouter;
        private readonly ICommandRouter _commandRouter;

        public BaseController(ILogger logger, IQueryRouter queryRouter, ICommandRouter commandRouter)
        {
            _logger = logger;
            _queryRouter = queryRouter;
            _commandRouter = commandRouter;
        }

        protected async Task<TResult> QueryAsync<TQuery, TResult>(TQuery query) where TQuery : IQuery<TResult>
        {
            return await _queryRouter.QueryAsync<TQuery, TResult>(query);
        }

        protected async Task<TResponse> RouteAsync<TCommand, TResponse>(TCommand cmd) where TCommand : ICommand<TResponse>
        {
            return await _commandRouter.RouteAsync<TCommand, TResponse>(cmd);
        }

        protected IActionResult ReadDefaultResponse(Response response, int statusCode = 500)
        {
            if (response.IsSuccessful)
            {
                return StatusCode(200);
            }
            else
            {
                return StatusCode(statusCode, response.Message);
            }
        }
    }
}