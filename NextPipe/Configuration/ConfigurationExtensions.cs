using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        public static IServiceCollection UseNextPipeDefaultConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
           services.UseMongoDBConfiguration(configuration);
           return services;
        }
    }
}