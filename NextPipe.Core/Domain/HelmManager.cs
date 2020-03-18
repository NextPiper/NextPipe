using System;

namespace NextPipe.Core
{
    public class HelmManager
    {
        public void InstallHelm(bool verboseInstallation = false)
        {
            "curl -fsSL -o get_helm.sh https://raw.githubusercontent.com/helm/helm/master/scripts/get-helm-3".Bash(verboseInstallation);
            "chmod 700 get_helm.sh".Bash(verboseInstallation);
            "./get_helm.sh".Bash(verboseInstallation);
        }

        public void InstallRabbitMQ(bool verboseInstallation = false)
        {
            "helm repo add bitnami https://charts.bitnami.com/bitnami".Bash(verboseInstallation);
            "helm repo update".Bash(verboseInstallation);
            "helm install rabbitmq bitnami/rabbitmq --set rabbitmq.username=\"admin\" --set rabbitmq.password=\"admin\" --set rabbitmq.erlangCookie=secretcookie --set rbacEnabled=true --set service.type=ClusterIP --set ingress.enabled=true --set ingress.hostName=\"rabbitmq.example.com\" --set ingress.tls=true --set ingress.tlsSecret=\"rabbitmq-tls\" --set ingress.annotations.\"kubernetes.io/ingress.class\"=\"nginx\" --set replicas=3".Bash(verboseInstallation);
        }
    }
}