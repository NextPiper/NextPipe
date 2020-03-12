using System;
using System.Threading.Tasks;

using k8s;

namespace NextPipe.Core
{
    public interface IKubernetesClient
    {
        Task InitClient();
    }
    
    public class KubernetesClient : IKubernetesClient
    {

        public KubernetesClient()
        {
            var config = KubernetesClientConfiguration.BuildDefaultConfig();
            
            Console.WriteLine(config.CurrentContext);
            Console.WriteLine(config.Host);
            
            Console.WriteLine("You are da boss");
        }
        
        public async Task InitClient()
        {
            
        }
    }
}