using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NextPipe.Core.Configurations;
using NextPipe.Core.Kubernetes;
using NextPipe.Core.Queries.Models;
using NextPipe.Core.Queries.Queries;
using SimpleSoft.Mediator;

namespace NextPipe.Core.Queries.Handlers
{
    public class CoreConfigQueryHandler : IQueryHandler<GetRabbitMQCredentialsQuery,RabbitMQConfig>
    {
        private readonly IKubectlHelper _kubectlHelper;
        private readonly IOptions<RabbitMQDeploymentConfiguration> _rabbitConfig;

        public CoreConfigQueryHandler(IKubectlHelper kubectlHelper, IOptions<RabbitMQDeploymentConfiguration> rabbitConfig)
        {
            _kubectlHelper = kubectlHelper;
            _rabbitConfig = rabbitConfig;
        }
        
        public async Task<RabbitMQConfig> HandleAsync(GetRabbitMQCredentialsQuery query, CancellationToken ct)
        {
            var rabbitService = await _kubectlHelper.FetchRabbitMQService(query.LoadBalancer);

            if (rabbitService != null)
            {
                return new RabbitMQConfig(rabbitService.Spec.ClusterIP, _rabbitConfig.Value.RabbitServiceUsername, _rabbitConfig.Value.RabbitServicePassword, 5672);
            }

            return null;
        }
    }
}