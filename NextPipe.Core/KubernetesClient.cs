using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Xml.Schema;
using k8s;

namespace NextPipe.Core
{
    public interface IKubernetesClient
    {
        Task InitClient();
    }
    
    public class KubernetesClient : IKubernetesClient
    {
        private IKubernetes _kubeClient;
        
        public KubernetesClient(IKubernetes kubeClient)
        {
            // Execute cmd line args within kubernetes
            _kubeClient = kubeClient;
        }
        
        public async Task InitClient()
        {
            // Use rabbitDeploymentManager to ensure that rabbitMQ infrastructure has been provisioned.
            var rabbitDeploymentManager = new RabbitDeploymentManager(_kubeClient, new RabbitDeploymentConfiguration(
                () => Console.WriteLine("Success"),
                () => Console.WriteLine("Failure"),
                2,
                6,
                30
                ));
            await rabbitDeploymentManager.Init(2, 6, 30);
            
        }

        private void RabbitMQInfrastructureFailure()
        {
            Console.WriteLine("RabbitMQ-Infrastructure failed to be provisioned");
        }

        private void RabbitMQReady()
        {
            Console.WriteLine("RabbitMQ-Infrastructure ready");
        }
        
    }
}


/*
 *
 * Console.WriteLine("kubectl version --client".Bash());
            Console.WriteLine("curl -fsSL -o get_helm.sh https://raw.githubusercontent.com/helm/helm/master/scripts/get-helm-3".Bash());
            Console.WriteLine("ls".Bash());
            Console.WriteLine("chmod 700 get_helm.sh".Bash());
            Console.WriteLine("./get_helm.sh".Bash());
            Console.WriteLine("kubectl cluster-info".Bash());
            Console.WriteLine("kubectl get ns".Bash());
            Console.WriteLine("helm version".Bash());
            Console.WriteLine("kubectl get pods".Bash());
            Console.WriteLine("helm repo add bitnami https://charts.bitnami.com/bitnami".Bash());
            Console.WriteLine("helm repo update".Bash());
            Console.WriteLine("helm install rabbitmq bitnami/rabbitmq --set rabbitmq.username=\"admin\" --set rabbitmq.password=\"admin\" --set rabbitmq.erlangCookie=secretcookie --set rbacEnabled=true --set service.type=ClusterIP --set ingress.enabled=true --set ingress.hostName=\"rabbitmq.example.com\" --set ingress.tls=true --set ingress.tlsSecret=\"rabbitmq-tls\" --set ingress.annotations.\"kubernetes.io/ingress.class\"=\"nginx\" --set replicas=3".Bash());
            Console.WriteLine("Done");
 *
 * 
 */

/*

// Check if RabbitMQ has been deployed...
            
            Console.WriteLine("Hello");
            //Console.WriteLine("helm install rabbitmq bitnami/rabbitmq --set rabbitmq.username=\"admin\" --set rabbitmq.password=\"admin\" --set rabbitmq.erlangCookie=secretcookie --set rbacEnabled=true --set service.type=ClusterIP --set ingress.enabled=true --set ingress.hostName=\"rabbitmq.example.com\" --set ingress.tls=true --set ingress.tlsSecret=\"rabbitmq-tls\" --set ingress.annotations.\"kubernetes.io/ingress.class\"=\"nginx\" --set replicas=3".Bash());
            
            
            //Console.WriteLine("curl https://raw.githubusercontent.com/kubernetes/helm/master/scripts/get".Bash());
            //Console.WriteLine("ls".Bash());
            
            // Chmod helm script
            //Console.WriteLine("chmod 700 get_helm.sh".Bash());
            
            
            //var installHelm = "cd .. && cd NextPipe.Core && sh helm.sh".Bash();
            
            //Console.WriteLine(installHelm);

            //var lsOutput = "cd .. && cd NextPipe.Core && ls".Bash();
            
            //Console.WriteLine(lsOutput);
            
            //var config = KubernetesClientConfiguration.BuildDefaultConfig();

            //Console.WriteLine(config.CurrentContext);
            //Console.WriteLine(config.Host);
            
            Console.WriteLine("You are da boss");



*/