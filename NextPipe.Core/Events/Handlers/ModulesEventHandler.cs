using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using k8s.Models;
using NextPipe.Core.Domain.Module.KubernetesModule;
using NextPipe.Core.Domain.Module.ModuleManagers;
using NextPipe.Core.Domain.Module.ValueObjects;
using NextPipe.Core.Domain.NextPipeTask.ValueObject;
using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Core.Events.Events.ArchiveEvents;
using NextPipe.Core.Events.Events.ModuleEvents;
using NextPipe.Core.Helpers;
using NextPipe.Core.Kubernetes;
using NextPipe.Messaging.Infrastructure.Contracts;
using NextPipe.Persistence.Entities;
using NextPipe.Persistence.Entities.ArchivedObjects;
using NextPipe.Persistence.Entities.NextPipeModules;
using NextPipe.Persistence.Repositories;
using SimpleSoft.Mediator;
using LoadBalancerConfig = NextPipe.Core.Domain.Module.KubernetesModule.LoadBalancerConfig;
using TaskStatus = NextPipe.Persistence.Entities.TaskStatus;

namespace NextPipe.Core.Events.Handlers
{
    public class ModulesEventHandler : 
        IEventHandler<InstallPendingModulesEvent>,
        IEventHandler<InstallModuleEvent>,
        IEventHandler<UninstallModuleEvent>,
        IEventHandler<CleanModulesReadyForUninstallEvent>,
        IEventHandler<HealthCheckModulesEvents>
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly ITasksRepository _tasksRepository;
        private readonly IModuleManager _moduleManager;
        private readonly IEventPublisher _eventPublisher;
        private readonly IKubectlHelper _kubectlHelper;
        private readonly IArchiveRepository _archiveRepository;
        
        public ModulesEventHandler(IModuleRepository moduleRepository, ITasksRepository tasksRepository, IModuleManager moduleManager, IEventPublisher eventPublisher, IKubectlHelper kubectlHelper)
        {
            _moduleRepository = moduleRepository;
            _tasksRepository = tasksRepository;
            _moduleManager = moduleManager;
            _eventPublisher = eventPublisher;
            _kubectlHelper = kubectlHelper;
        }
        
        public async Task HandleAsync(InstallPendingModulesEvent evt, CancellationToken ct)
        {
            LogHandler.WriteLineVerbose($"{nameof(InstallPendingModulesEvent)} received - Search and install all modules of {nameof(ModuleStatus)}.{nameof(ModuleStatus.Pending)}");

            var result = await _moduleRepository.GetModulesByModuleStatus(ModuleStatus.Pending);

            if (!result.Any())
            {
                LogHandler.WriteLineVerbose($"No modules are pending for installation - Exiting {nameof(InstallPendingModulesEvent)}");
                return;
            }
            
            // Install the pending modules
            foreach (var module in result)
            {
                await HandleAsync(new InstallModuleEvent(new Id(module.Id)), ct);
            }
        }
        
        public async Task HandleAsync(CleanModulesReadyForUninstallEvent evt, CancellationToken ct)
        {
            LogHandler.WriteLineVerbose($"{nameof(CleanModulesReadyForUninstallEvent)} received - Search and install all modules of {nameof(ModuleStatus)}.{nameof(ModuleStatus.Uninstall)}");

            var result = await _moduleRepository.GetModulesByModuleStatus(ModuleStatus.Uninstall);

            if (!result.Any())
            {
                LogHandler.WriteLineVerbose($"No modules are waiting to be uninstalled - Exiting {nameof(CleanModulesReadyForUninstallEvent)}");
                return;
            }
            
            // Uninstall the modules of status uninstall
            foreach (var module in result)
            {
                await HandleAsync(new UninstallModuleEvent(new Id(module.Id)), ct);
            }
        }


        public async Task HandleAsync(InstallModuleEvent evt, CancellationToken ct)
        {
            // Fetch module first
            var module = await _moduleRepository.GetById(evt.ModuleId.Value);
            
            // Update module status to installing
            await _moduleRepository.UpdateModuleStatus(module.Id, ModuleStatus.Installing);
            
            // Create a task to handle this installation
            NextPipeTask task = await AttachToTaskOrStartNew(evt.TaskId, new Id(module.Id), TaskType.ModuleInstall);
            
            _moduleManager.SetVerboseLogging(true);
            await _moduleManager.DeployModule(new ModuleManagerConfig(
                new Id(task.TaskId), 
                new ModuleReplicas(module.DesiredReplicas),
                new ModuleName(module.ModuleName),
                new ImageName(module.ImageName),
                new LoadBalancerConfig(module.LoadBalancerConfig.NeedLoadBalancer,module.LoadBalancerConfig.Port, module.LoadBalancerConfig.TargetPort), 
                async (id, logHandler) =>
                {
                    await _moduleRepository.UpdateModuleStatus(module.Id, ModuleStatus.Running);
                    await _tasksRepository.FinishTask(id.Value, TaskStatus.Success, logHandler.GetLog());
                },
                async (id, logHandler) =>
                {
                    await _moduleRepository.UpdateModuleStatus(module.Id, ModuleStatus.Failed);
                    await _tasksRepository.FinishTask(id.Value, TaskStatus.Failed, logHandler.GetLog());
                },
                async (id, logHandler) => { await _tasksRepository.AppendLog(id.Value, logHandler.GetLog()); }));
        }

        public async Task HandleAsync(UninstallModuleEvent evt, CancellationToken ct)
        {
            var removeModule = await _moduleRepository.GetById(evt.ModuleId.Value);
            await _moduleRepository.UpdateModuleStatus(removeModule.Id, ModuleStatus.Uninstalling);
            
            NextPipeTask task = await AttachToTaskOrStartNew(evt.TaskId, new Id(removeModule.Id), TaskType.ModuleUninstall);
            
            _moduleManager.SetVerboseLogging(true);

            // Maybe cleanup such that we do not need to pass alot of parameters which are redundant. We could make same design as rabbitDeploymentManager...
            await _moduleManager.UninstallModule(new ModuleManagerConfig(evt.TaskId, new ModuleReplicas(removeModule.DesiredReplicas), 
                new ModuleName(removeModule.ModuleName), new ImageName(removeModule.ImageName), new LoadBalancerConfig(removeModule.LoadBalancerConfig.NeedLoadBalancer, removeModule.LoadBalancerConfig.Port, removeModule.LoadBalancerConfig.TargetPort), 
                async (id, logHandler) =>
                {
                    await _moduleRepository.SetModuleStatus(removeModule.Id, ModuleStatus.Uninstalled);
                    await _tasksRepository.FinishTask(task.TaskId, TaskStatus.Success, logHandler.GetLog());
                    _eventPublisher.PublishAsync(new ArchiveModuleEvent(new Id(removeModule.Id)), ct);
                },
                async (id, logHandler) =>
                {
                    await _moduleRepository.SetModuleStatus(removeModule.Id, ModuleStatus.FailedUninstall);
                    await _tasksRepository.FinishTask(task.TaskId, TaskStatus.Failed, logHandler.GetLog());
                },
                async (id, logHandler) => { await _tasksRepository.AppendLog(task.TaskId, logHandler.GetLog()); }));
        }

        private async Task<NextPipeTask> AttachToTaskOrStartNew(Id taskId, Id moduleId, TaskType taskType)
        {
            NextPipeTask task;
            if (taskId != null)
            {
                task = await _tasksRepository.GetTaskByTaskId(taskId.Value);
                _moduleManager.AttachPreviousLogs(task.Logs);
            }
            else
            {
                task = new NextPipeTask
                {
                    Id = new Id().Value,
                    Hostname = new Hostname().Value,
                    ReferenceId = moduleId.Value,
                    TaskId = new Id().Value,
                    TaskStatus = TaskStatus.Running,
                    TaskType = taskType,
                    QueueStatus = QueueStatus.Running,
                    Metadata = null,
                    TaskPriority = TaskPriority.Medium
                };
                await _tasksRepository.Insert(task);
            }

            return task;
        }

        public async Task HandleAsync(HealthCheckModulesEvents evt, CancellationToken ct)
        {
            // Get all running modules and their respective replicas
            var liveModules = await _kubectlHelper.GetLiveModules();
            var npModules = await _moduleRepository.GetRunningModules();
            
            // Update health status from kubernetes
            await UpdateHealthStatus(liveModules, npModules);
            // Check that the desired number of replicas is reflected in kubernetes
            await ValidateReplicaAggrement(liveModules, npModules);
        }

        private async Task UpdateHealthStatus(IEnumerable<KubernetesModule> liveModules, IEnumerable<Module> npModules)
        {
            // Update number of ready replicas - if a replica is not ready check if there is any status why and update as well
            foreach (var npModule in npModules)
            {
                // Find the corresponding live module
                var liveModule = liveModules.SingleOrDefault(t => t.Deployment.Metadata.Name.Equals(npModule.ModuleName));

                var replicaStatuses = new List<ReplicaStatus>();
                
                if (liveModule != null)
                {
                    // We have found the corresponding liveModule - do all cross checking and log fetching, then update repository in the end...
                    // <----
                    var checkList = new List<V1Pod>(liveModule.Pods);
                    
                    // Foreach replica pod in the npModule update its content
                    foreach (var replicaLog in npModule.ReplicaLogs)
                    {
                        // Check if pod is still alive
                        var alive = checkList.SingleOrDefault(t => t.Metadata.Name.Equals(replicaLog.DeploymentId));

                        if (alive != null) // If pod is alive update replicaStatus and read "kubectl pod describe" and "kubectl logs <pod name>"
                        {
                            replicaLog.IsAlive = true;
                            replicaLog.Status = alive.Status.Phase;
                            replicaLog.PodLog = GetPodLogs(alive.Metadata.Name, replicaLog);
                            replicaLog.PodDescribe = GetPodDescription(alive.Metadata.Name, replicaLog);
                            checkList.Remove(alive);
                        }
                        else
                        {
                            replicaLog.IsAlive = false;
                            replicaLog.Status = "Pod killed";
                            replicaLog.PodLog = GetPodLogs(replicaLog.DeploymentId, replicaLog);
                            replicaLog.PodDescribe = GetPodDescription(replicaLog.DeploymentId, replicaLog);
                        }
                    }

                    foreach (var newPod in checkList)
                    {
                        npModule.ReplicaLogs.Add(new ReplicaStatus
                        {
                            DeploymentId = newPod.Metadata.Name,
                            IsAlive = true,
                            Status = newPod.Status.Phase,
                            PodDescribe = GetPodDescription(newPod.Metadata.Name),
                            PodLog = GetPodLogs(newPod.Metadata.Name)
                        });
                    }
                    
                    // Check loadBalancer services for moduleRepository.
                    var service = await _kubectlHelper.GetService($"{npModule.ModuleName}-service");

                    var loadBalancer = UpdateLoadBalancerStatus(service, npModule);
                    
                    await _moduleRepository.UpdateHealthStatus(npModule.Id,
                        liveModule.Deployment.Status.ReadyReplicas.Value, npModule.ReplicaLogs, loadBalancer);
                }
            } 
        }

        private LoadBalancer UpdateLoadBalancerStatus(V1Service service, Module module)
        {
            if (service == null)
            {
                return module.LoadBalancer;
            }

            return new LoadBalancer
            {
                ExternalIPs = service.Spec.ExternalIPs,
                Ports = service.Spec.Ports.Select(t => t.Port)
            };
        }

        private async Task ValidateReplicaAggrement(IEnumerable<KubernetesModule> liveModules, IEnumerable<Module> npModules)
        {
            foreach (var npModule in npModules)
            {
                // Find the corresponding live module
                var liveModule = liveModules.SingleOrDefault(t => t.Deployment.Metadata.Name.Equals(npModule.ModuleName));

                if (liveModule != null) // Found the corresponding module
                {
                    if (liveModule.Deployment.Spec.Replicas.Value != npModule.DesiredReplicas)
                    {
                        // Desired replicas has changed update kubernetes accordingly
                        var response = await _kubectlHelper.ScaleDeployment(liveModule.Deployment.Metadata.Name,
                            new ModuleReplicas(npModule.DesiredReplicas));

                        if (!response.IsSuccessful)
                        {
                            // Log that we tried to scale the module but it did not work
                            await _moduleRepository.AppendLog(npModule.Id,
                                npModule.Logs +
                                $"\nTried to scale module replicas from {liveModule.Deployment.Spec.Replicas.Value} to {npModule.DesiredReplicas} but received an error: \n ***** \n {response.Message} \n*****\n");
                        }
                    }
                }
            }
        }

        private string GetPodLogs(string podName, ReplicaStatus status = null)
        {
            var podLogs = $"kubectl logs {podName}".Bash();

            if (podLogs.IdenticalStart("Error from server (NotFound):"))
            {
                return status != null ? status.PodLog : "";
            }

            return podLogs;
        }

        private string GetPodDescription(string podName, ReplicaStatus status = null)
        {
            var podDescribe = $"kubectl describe pod {podName}".Bash();

            if (podDescribe.IdenticalStart("Error from server (NotFound):"))
            {
                return status != null ? status.PodDescribe : "";
            }

            return podDescribe;
        }
    }
}