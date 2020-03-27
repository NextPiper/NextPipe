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
    public class ModuleStateManagerService : IHostedService, IDisposable
    {
        private readonly ICommandRouter _commandRouter;
        private bool IsRunning;

        public ModuleStateManagerService(ICommandRouter commandRouter)
        {
            _commandRouter = commandRouter;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            IsRunning = true;
            ResolvePendingModules(cancellationToken);
            ResolveUninstallModules(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            IsRunning = false;
        }

        private async Task ResolvePendingModules(CancellationToken cancellationToken)
        {
            while (IsRunning)
            {
                await Task.Delay(15.SecToMillis(), cancellationToken);
                
                Console.WriteLine("Scheduling long running task for installing pending modules");

                var result =
                    await _commandRouter.RouteAsync<InstallPendingModulesCommand, Response>(
                        new InstallPendingModulesCommand(),cancellationToken);
            }
        }

        private async Task ResolveUninstallModules(CancellationToken cancellationToken)
        {
            while (IsRunning)
            {
                await Task.Delay(30.SecToMillis(), cancellationToken);
                
                Console.WriteLine("Scheduling long running task for uninstalling modules");
                var result =
                    await _commandRouter.RouteAsync<CleanModulesReadyForUninstallCommand, Response>(
                        new CleanModulesReadyForUninstallCommand(), cancellationToken);

            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}