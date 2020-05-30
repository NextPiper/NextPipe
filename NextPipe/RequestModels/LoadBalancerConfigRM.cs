using System.Reflection.Metadata.Ecma335;

namespace NextPipe.RequestModels
{
    public class LoadBalancerConfigRM
    {

        /// <summary>
        /// If set to true, a services of type loadbalander will be created for this module
        /// </summary>
        public bool NeedLoadBalancer { get; set; } = false;
        /// <summary>
        /// If set to zero a random available port will be assigned
        /// </summary>
        public int Port { get; set; } = 0;
        /// <summary>
        /// Default targetport of container is 80
        /// </summary>
        public int TargetPort { get; set; } = 80;
    }
}