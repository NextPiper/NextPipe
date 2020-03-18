using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NextPipe.Core;

namespace NextPipe.Services
{
    public class KubernetesService : IHostedService, IDisposable
    {
        private readonly IKubernetesClient _kubernetesClient;

        public KubernetesService(IKubernetesClient kubernetesClient)
        {
            _kubernetesClient = kubernetesClient;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _kubernetesClient.InitClient();
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