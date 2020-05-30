using NextPipe.Core.Domain.SharedValueObjects;

namespace NextPipe.Core.Domain.Module.KubernetesModule
{
    public class LoadBalancerConfig
    {
        public bool NeedLoadBalancer { get; }
        public int Port { get; }
        public int TargetPort { get; }
        
        public LoadBalancerConfig(bool needLoadBalancer, int port, int targetPort)
        {
            NeedLoadBalancer = needLoadBalancer;
            Port = port;
            TargetPort = targetPort;
        }
    }
}