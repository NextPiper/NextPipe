using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NextPipe.Core.Configurations;
using NextPipe.Core.Helpers;

namespace NextPipe.Core
{
    public interface IHelmManager
    {
        Task InstallHelm(ILogHandler logHandler, bool verboseInstallation = false);
        Task InstallRabbitMQ(string deploymentName, ILogHandler logHandler, bool verboseInstallation = false, int replicas = 3);
        Task CleanUp(string deploymentName,ILogHandler logHandler, bool verboseCleanup = false);
    }
    
    public class HelmManager : IHelmManager
    {
        private readonly IOptions<RabbitMQDeploymentConfiguration> _rabbitConfig;

        public HelmManager(IOptions<RabbitMQDeploymentConfiguration> rabbitConfig)
        {
            _rabbitConfig = rabbitConfig;
        }

        public async Task InstallHelm(ILogHandler logHandler, bool verboseInstallation = false)
        {
            await "kubectl version --client".BashAsync(logHandler, verboseInstallation);
            await "curl -fsSL -o get_helm.sh https://raw.githubusercontent.com/helm/helm/master/scripts/get-helm-3"
                .BashAsync(logHandler, verboseInstallation);
            await "chmod 700 get_helm.sh".BashAsync(logHandler, verboseInstallation);
            await "./get_helm.sh".BashAsync(logHandler, verboseInstallation);
        }

        public async Task InstallRabbitMQ(string deploymentName, ILogHandler logHandler, bool verboseInstallation = false, int replicas = 3)
        {
            await "helm repo add bitnami https://charts.bitnami.com/bitnami".BashAsync(logHandler, verboseInstallation);
            await "helm repo update".BashAsync(logHandler, verboseInstallation);
            await $"helm install {deploymentName} bitnami/rabbitmq --set rabbitmq.username=\"{_rabbitConfig.Value.RabbitServiceUsername}\" --set rabbitmq.password=\"{_rabbitConfig.Value.RabbitServicePassword}\" --set rabbitmq.erlangCookie=secretcookie --set rbacEnabled=true --set service.type=ClusterIP --set ingress.enabled=true --set ingress.hostName=\"rabbitmq.example.com\" --set ingress.tls=true --set ingress.tlsSecret=\"rabbitmq-tls\" --set ingress.annotations.\"kubernetes.io/ingress.class\"=\"nginx\" --set replicas={replicas}"
                    .BashAsync(logHandler, verboseInstallation);
        }

        public async Task CleanUp(string deploymentName, ILogHandler logHandler, bool verboseCleanup = false)
        {
            await $"helm delete {deploymentName}".BashAsync(logHandler, verboseCleanup);
        }
    }
}