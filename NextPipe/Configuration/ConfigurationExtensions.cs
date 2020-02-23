using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NextPipe.Configuration
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection UseKubernetesConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<KubernetesConfiguration>(configuration.GetSection(nameof(KubernetesConfiguration)));
            return services;
        }

        public static IServiceCollection UseNextPipeDefaultConfiguration(this IServiceCollection services,
            IConfiguration configuration)
        {
           services.UseKubernetesConfiguration(configuration);
           return services;
        }
    }
}