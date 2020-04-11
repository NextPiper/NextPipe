using System.Collections.Generic;

namespace NextPipe.Persistence.Entities.NextPipeModules
{
    public class LoadBalancer
    {
        public IEnumerable<int> Ports { get; set; } = new List<int>();
        public IEnumerable<string> ExternalIPs { get; set; } = new List<string>();
    }
}