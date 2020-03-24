using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NextPipe.Core.Commands.Commands.ProcessLockCommands;
using NextPipe.Core.Domain.NextPipeTask.ValueObject;
using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Core.Events.Events;
using NextPipe.Core.Helpers;
using NextPipe.Core.Kubernetes;
using NextPipe.Messaging.Infrastructure.Contracts;
using NextPipe.Persistence.Entities.ProcessLock;
using NextPipe.Persistence.Repositories;
using NextPipe.Utilities.Documents.Responses;
using SimpleSoft.Mediator;

namespace NextPipe.Core.Commands.Handlers
{
    public class BackgroundProcessCommandHandler : CommandHandlerBase,
        ICommandHandler<CleanupHangingTasksCommand, Response>
    {
        private readonly IProcessLockRepository _processLockRepository;
        private readonly IKubectlHelper _kubectlHelper;
        private const string NEXTPIPE_DEPLOYMENT_NAME = "nextpipe-deployment";

        public BackgroundProcessCommandHandler(IEventPublisher eventPublisher, IProcessLockRepository processLockRepository, IKubectlHelper kubectlHelper) : base(eventPublisher)
        {
            _processLockRepository = processLockRepository;
            _kubectlHelper = kubectlHelper;
        }

        /// <summary>
        /// Request a processLock for the respective host
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public async Task<Response> HandleAsync(CleanupHangingTasksCommand cmd, CancellationToken ct)
        {
            // Check if there exists a Process of respective type which is already running...
            Console.WriteLine($"Request processLock for processType: {nameof(NextPipeProcessType.CleanUpHangingTasks)}");
            var processLock = await RequestProcessLock(NextPipeProcessType.CleanUpHangingTasks);

            if (processLock == null)
            {
                // Request for process lock was not successful return unsuccesfull cmd and try new cleanup in 30 secs
                Console.WriteLine("Couldn't receive processLock, waiting for next clean up session");
                return Response.Unsuccessful();
            }
            
            Console.WriteLine("Process lock received publishing cleanup event!");
            
            // Successfully created process lock for this host for the requested processType
            await _eventPublisher.PublishAsync(new CleanupHangingTasksEvent(), ct);

            Console.WriteLine("Clean up process done, Delete processLock");
            // The process is done, remove the processLock
            await _processLockRepository.Delete(processLock.Id);
            
            return Response.Success();
        }


        /// <summary>
        /// Returns null if the method was not able to assign a processLock. Else returns a processLock
        /// </summary>
        /// <param name="processType"></param>
        /// <returns></returns>
        private async Task<ProcessLock> RequestProcessLock(NextPipeProcessType processType)
        {
            // Find process of processType
            var process =
                await _processLockRepository.FindProcessLockByProcessType(processType);
            
            if (process != null)
            {
                Console.WriteLine("Process already running...");
                // The process is already running - Make sure that the processLock is not assigned to a dead host
                var hostPods =
                    await _kubectlHelper.GetPodsByCustomNameFilter(NEXTPIPE_DEPLOYMENT_NAME, ShellHelper.IdenticalStart);

                if (hostPods.All(t => t.Metadata.Name != process.Hostname))
                {
                    Console.WriteLine("Process was hanging on dead host. Rescheduling the process to this host");
                    // The hostname of the process does not match any of the current hosts
                    // Re-schedule the CleanupHangingTaskCommand by deleting and inserting a new processLock
                    // Attached to this host
                    return await _processLockRepository.ReplaceProcessLock(new ProcessLock
                    {
                        Hostname = new Hostname().Value,
                        Id = new Id().Value,
                        ProcessId = new Id().Value,
                        NextPipeProcessType = NextPipeProcessType.CleanUpHangingTasks
                    }, process);
                } 
            }
            else
            {
                Console.WriteLine("No process was running trying to request processLock");
                // The process is not running, create a processLock for this host
                // This might fail if another replica beats us to the finish line
                return await _processLockRepository.InsertAndReturn(new ProcessLock
                {
                    Hostname = new Hostname().Value,
                    Id = new Id().Value,
                    ProcessId = new Id().Value,
                    NextPipeProcessType = NextPipeProcessType.CleanUpHangingTasks
                });
            }
            return null;
        }
    }
}