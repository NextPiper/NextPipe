using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NextPipe.Core.Configurations;
using NextPipe.Persistence.Configuration;

namespace NextPipe.Configuration
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection UseMongoDBConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoDBPersistenceConfiguration>(configuration.GetSection(nameof(MongoDBPersistenceConfiguration)));
            return services;
        }

        public static IServiceCollection UseRabbitMQDeploymentConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
            services.Configure<RabbitMQDeploymentConfiguration>(
                configuration.GetSection(nameof(RabbitMQDeploymentConfiguration)));
            return services;
        }

        public static IServiceCollection UseNextPipeDefaultConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
           services.UseMongoDBConfiguration(configuration);
           services.UseRabbitMQDeploymentConfiguration(configuration);
           return services;
        }
    }
}