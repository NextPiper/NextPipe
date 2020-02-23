using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace NextPipe.Services
{
    public class KubernetesService : IHostedService, IDisposable
    {

        public KubernetesService()
        {
            
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(5000);
                Console.WriteLine("Magunss");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Console.WriteLine("Clean up nice resources");
        }
    }
}