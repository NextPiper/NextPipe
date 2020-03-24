using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NextPipe.Core.Commands.Commands.ProcessLockCommands;
using NextPipe.Core.Helpers;
using NextPipe.Messaging.Infrastructure.Contracts;
using NextPipe.Utilities.Documents.Responses;

namespace NextPipe.Services
{
    public class ResourceAndStateManagerService : IHostedService, IDisposable
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly ICommandRouter _commandRouter;
        private bool isRunning;
        
        // Ask for options to configure loop
        public ResourceAndStateManagerService(ICommandRouter commandRouter)
        {
            _commandRouter = commandRouter;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            isRunning = true;
            
            // Start the taskResourceCleaner thread 
            TaskResourceCleaner(cancellationToken);   
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            isRunning = false;
        }

        private async Task TaskResourceCleaner(CancellationToken cancellationToken)
        {
            while (isRunning)
            {
                // Every 60 seconds starts a TaskResourceCleanupTask
                await Task.Delay(1.MinToMillis(), cancellationToken);

                Console.WriteLine("Scheduling CleanupHangingTasksCommand");
                var result = await _commandRouter.RouteAsync<CleanupHangingTasksCommand, Response>(
                    new CleanupHangingTasksCommand(), cancellationToken);
            }
        }
        
        public void Dispose()
        {
        }
    }
}