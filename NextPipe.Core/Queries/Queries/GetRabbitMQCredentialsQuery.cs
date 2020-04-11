using NextPipe.Core.Queries.Models;

namespace NextPipe.Core.Queries.Queries
{
    public class GetRabbitMQCredentialsQuery : BaseQuery<RabbitMQConfig>
    {
        public bool LoadBalancer { get; }

        public GetRabbitMQCredentialsQuery(bool loadBalancer = false)
        {
            LoadBalancer = loadBalancer;
        }
    }
}