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
            //var config = KubernetesClientConfiguration.BuildConfigFromConfigFile("/Users/ulriksandberg/Desktop/NextPipeKubeConfig.yml");
            var config = KubernetesClientConfiguration.BuildDefaultConfig();
            
            Console.WriteLine("Heja");
        }
        
        public async Task InitClient()
        {
            
        }
    }
}