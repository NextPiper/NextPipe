namespace NextPipe.Persistence.Entities.NextPipeModules
{
    public class LoadBalancerConfig
    {
        public bool NeedLoadBalancer { get; set; }
        public int Port { get; set; }
        public int TargetPort { get; set; }
    }
}