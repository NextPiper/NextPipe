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
            
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            IsRunning = false;
            
        }

        private async Task ResolveModulesWithRequestToChange(CancellationToken cancellationToken)
        {
            while (IsRunning)
            {
                await Task.Delay(1.MinToMillis(), cancellationToken);
                
                Console.WriteLine("Running check to change models with states: Pending, Uninstall");

                var result =
                    await _commandRouter.RouteAsync<ResolveModuleWithRequestForChangeCommand, Response>(
                        new ResolveModuleWithRequestForChangeCommand(),cancellationToken);

            }
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}