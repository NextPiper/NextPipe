using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NextPipe.Core.Configurations;
using NextPipe.Core.Helpers;
using NextPipe.Utilities.Core;

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
            /*var downloadHelmResult =
                "curl -fsSL -o get_helm.sh https://raw.githubusercontent.com/helm/helm/master/scripts/get-helm-3"
                    .Bash(true);
            await logHandler.WriteLine(downloadHelmResult, verboseInstallation);
            var giveHelmScriptExecutionRightsResult = "chmod 700 get_helm.sh".Bash(true);
            await logHandler.WriteLine(giveHelmScriptExecutionRightsResult, verboseInstallation);
            var executeHelmScriptResult = "./get_helm.sh".Bash(true);
            await logHandler.WriteLine(executeHelmScriptResult, verboseInstallation);*/
            Console.WriteLine("kubectl version --client".Bash());
            Console.WriteLine("curl -fsSL -o get_helm.sh https://raw.githubusercontent.com/helm/helm/master/scripts/get-helm-3".Bash());
            Console.WriteLine("ls".Bash());
            Console.WriteLine("chmod 700 get_helm.sh".Bash());
            Console.WriteLine("./get_helm.sh".Bash());
        }

        public async Task InstallRabbitMQ(string deploymentName, ILogHandler logHandler, bool verboseInstallation = false, int replicas = 3)
        {
            var helmAddRepoResult = "helm repo add bitnami https://charts.bitnami.com/bitnami".Bash(true);
            await logHandler.WriteLine(helmAddRepoResult, verboseInstallation);
            var helmRepoUpdateResult = "helm repo update".Bash(true);
            await logHandler.WriteLine(helmRepoUpdateResult, verboseInstallation);
            var helmInstallRabbitResult =
                $"helm install {deploymentName} bitnami/rabbitmq --set rabbitmq.username=\"{_rabbitConfig.Value.RabbitServiceUsername}\" --set rabbitmq.password=\"{_rabbitConfig.Value.RabbitServicePassword}\" --set rabbitmq.erlangCookie=secretcookie --set rbacEnabled=true --set service.type=ClusterIP --set ingress.enabled=true --set ingress.hostName=\"rabbitmq.example.com\" --set ingress.tls=true --set ingress.tlsSecret=\"rabbitmq-tls\" --set ingress.annotations.\"kubernetes.io/ingress.class\"=\"nginx\" --set replicas={replicas}".Bash(true);
            await logHandler.WriteLine(helmInstallRabbitResult, verboseInstallation);
        }

        public async Task CleanUp(string deploymentName, ILogHandler logHandler, bool verboseCleanup = false)
        {
            await logHandler.WriteLine("Infrastructure provision failed --> Cleaning up resources", verboseCleanup);
            await logHandler.WriteLine($"helm uninstall {deploymentName}".Bash(), verboseCleanup);
        }
    }
}