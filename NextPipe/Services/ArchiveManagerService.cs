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
    public class ArchiveManagerService : IHostedService, IDisposable
    {
        private readonly ICommandRouter _commandRouter;
        private bool IsRunning;

        public ArchiveManagerService(ICommandRouter commandRouter)
        {
            _commandRouter = commandRouter;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            IsRunning = true;

            StartArchiveUninstalledModulesBackgroundProcess(cancellationToken);
            StartArchiveCompletedTasksBackgroundProcess(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            IsRunning = false;
        }

        private async Task StartArchiveUninstalledModulesBackgroundProcess(CancellationToken cancellationToken)
        {
            while (IsRunning)
            {
                await Task.Delay(30.SecToMillis() + new Random().Next(0,10), cancellationToken);
                
                Console.WriteLine("Scheduling long running task for archiving uninstalled modules");

                var result =
                    await _commandRouter.RouteAsync<ArchiveModulesCommand, Response>(new ArchiveModulesCommand(),
                        cancellationToken);
            }
        }

        private async Task StartArchiveCompletedTasksBackgroundProcess(CancellationToken cancellationToken)
        {
            while (IsRunning)
            {
                await Task.Delay(1.MinToMillis() + new Random().Next(0,10), cancellationToken);
                
                Console.WriteLine("Scheduling long running task for archiving completed tasks");

                var result =
                    await _commandRouter.RouteAsync<ArchiveTasksCommand, Response>(new ArchiveTasksCommand(),
                        cancellationToken);
            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}