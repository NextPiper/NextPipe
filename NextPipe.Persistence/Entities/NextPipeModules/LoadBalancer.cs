using System.Collections.Generic;

namespace NextPipe.Persistence.Entities.NextPipeModules
{
    public class LoadBalancer
    {
        public IEnumerable<int> Ports { get; set; } = new List<int>();
        public IEnumerable<LoadBalancerExternal> ExternalIPs { get; set; } = new List<LoadBalancerExternal>();
    }
}