using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Options;
using NextPipe.Core.Configurations;
using NextPipe.Core.Documents;
using NextPipe.Core.Helpers;
using NextPipe.Core.Kubernetes;
using NextPipe.Utilities.Core;
using NextPipe.Utilities.Resources;

namespace NextPipe.Core
{
    public interface IRabbitDeploymentManager
    {
        bool IsInfrastructureRunning(int lowerBoundaryReadyReplicas);
        Task Deploy(IRabbitDeploymentManagerConfiguration config);
    }

    public class RabbitDeploymentManager : IRabbitDeploymentManager
    {
        private const string RABBIT_MQ_STATEFULSET = "rabbitmq";
        private const string RABBIT_MQ_PVC = "data-rabbitmq-";
        private const string RABBIT_MQ_SERVICE = "rabbitmq-service";
        private readonly IKubectlHelper _kubectlHelper;
        private readonly IOptions<RabbitMQDeploymentConfiguration> _rabbitConfig;
        private readonly IHelmManager _helmManager;
        private readonly ILogHandler _logHandler;

        public RabbitDeploymentManager(IKubectlHelper kubectlHelper, IOptions<RabbitMQDeploymentConfiguration> rabbitConfig, IHelmManager helmManager)
        {
            _kubectlHelper = kubectlHelper;
            _rabbitConfig = rabbitConfig;
            _helmManager = helmManager;
            _logHandler = new LogHandler();
        }
    
        /// <summary>
        /// Return a bool indicating if the infra
        /// </summary>
        /// <param name="lowerBoundaryReadyReplicas"></param>
        /// <returns></returns>
        public bool IsInfrastructureRunning(int lowerBoundaryReadyReplicas)
        {
            var rabbitStatefulSetIsRunning = _kubectlHelper.ValidateStatefulsetIsRunning(RABBIT_MQ_STATEFULSET);
    
            if (rabbitStatefulSetIsRunning)
            {
                var numberOfReadyReplicas = _kubectlHelper.GetNumberOfStatefulsetReadyReplicas(RABBIT_MQ_STATEFULSET);
                if (numberOfReadyReplicas >= lowerBoundaryReadyReplicas)
                {
                    return true;
                }
            }
    
            return false;
        }
        
        /// <summary>
        /// Validate and or provision the rabbitMQ infrastructure.  
        /// </summary>
        /// <param name="lowerBoundaryReplicas"></param>
        /// <param name="failureThreshold"></param>
        /// <param name="trialsDelaySec"></param>
        /// <returns></returns>
        public async Task Deploy(IRabbitDeploymentManagerConfiguration config)
        {
            // Attach taskId and updateCallback to the logHandler
            _logHandler.AttachTaskIdAndUpdateHandler(config.TaskId, config.UpdateCallback);
            await Deploy(config, false);
        }

        private async Task Deploy(IRabbitDeploymentManagerConfiguration config, bool abortOnFailure = false)
        {
            // Check if statefulset is already deployed
            var rabbitStatefulSetIsRunning = _kubectlHelper.ValidateStatefulsetIsRunning(RABBIT_MQ_STATEFULSET);
            await _logHandler.WriteLine($"{nameof(RabbitDeploymentManager)}.{nameof(Deploy)} --> Validating RabbitMQ infrastructure", true);
            
            if (rabbitStatefulSetIsRunning)
            {
                await ValidateRabbitMQDeployment(config);
            }
            else
            {
                if (abortOnFailure)
                {
                    await Cleanup();
                    await config.FailureCallback(config.TaskId, _logHandler);
                    return;
                }

                await InstallHelm();
                // Once helm has installed and rabbitMQ has been provisioned to the cluster by helm retry the init call
                // else abort the process...
                await Deploy(config, true);
            }
        }

        /// <summary>
        /// Returns true if we 
        /// </summary>
        /// <returns></returns>
        private async Task CreateRabbitMQService()
        {
            if (_rabbitConfig.Value.IsRabbitServiceEnabled)
            {
                await _logHandler.WriteLine("Installing RabbitMQ Load Balancer Service", true);
                await _kubectlHelper.InstallService(KubectlHelper.GetRabbitMQService());
            }
        }

        private async Task Cleanup()
        {
            // Uninstall helm
            _helmManager.CleanUp(RABBIT_MQ_STATEFULSET, _logHandler, true);
            
            // Uninstall rabbitmq service
            await _logHandler.WriteLine("Removing RabbitMQ Loadbalancer Service",true);
            await _kubectlHelper.DeleteService(RABBIT_MQ_SERVICE);
            
            // Clean-up pvc!
            _logHandler.WriteLine("Cleaning Persistent Volume Claims from rabbitMQ", true);
            var pvcList = await _kubectlHelper.GetPVCsByCustomerNameFilter(RABBIT_MQ_PVC, ShellHelper.IdenticalStart);
            foreach (var pvc in pvcList)
            {
                _logHandler.WriteLine($"Deleting PVC: {pvc.Metadata.Name}");
            }
            await _kubectlHelper.DeletePVCList(pvcList);
        }

        private async Task InstallHelm()
        {
            await _logHandler.WriteLine("No existing RabbitMQ infrastructure --> Provision RabbitMQ infrastructure", true);
            _helmManager.InstallHelm(_logHandler, true);
            _helmManager.InstallRabbitMQ(RABBIT_MQ_STATEFULSET, _logHandler, true);
            await Task.Delay(30.ToMillis());
        }

        private async Task ValidateRabbitMQDeployment(IRabbitDeploymentManagerConfiguration config)
        {
            await _logHandler.WriteLine($"RabbitMQ Service deployed --> Checking ready nodes", true);
            // Validate that at least lowerBoundaryReplicas are running for availability across the cluster
            var isClusterReady = await WaitForLowerBoundaryReplicas(config, RABBIT_MQ_STATEFULSET);
            if (isClusterReady)
            {
                await _logHandler.WriteLine("RabbitMQ Cluster is ready --> The rabbitMQ cluster has been provisioned and lowerBoundaryReplicasMet=true", true);
                await CreateRabbitMQService();
                await config.SuccessCallback(config.TaskId, _logHandler);
            }
            else
            {
                await Cleanup();
                await config.FailureCallback(config.TaskId, _logHandler);
            }
        }
        
        private async Task<bool> WaitForLowerBoundaryReplicas(IRabbitDeploymentManagerConfiguration config, string statefulSetname, string nameSpace = "default")
        {
            // true as long as none of the constraints are met
            var failedAttempts = 0;
    
            var readyReplicas = _kubectlHelper.GetNumberOfStatefulsetReadyReplicas(statefulSetname, nameSpace);
            await _logHandler.WriteLine($"lowerBoundaryReplicas={config.LowerBoundaryReadyReplicas}, readyReplicas={readyReplicas}", true);
            
            if (readyReplicas >= config.LowerBoundaryReadyReplicas)
            {
                return true;
            }
    
            await _logHandler.WriteLine("Waiting for ready replicas...", true);
    
            // Wait the initial delay
            await Task.Delay(config.ReplicaDelaySeconds.ToMillis());
    
            while (true)
            {
                var rReplicas = _kubectlHelper.GetNumberOfStatefulsetReadyReplicas(statefulSetname, nameSpace);
                if (rReplicas >= config.LowerBoundaryReadyReplicas)
                {
                    return true;
                }
    
                // Increment the failed attempts
                failedAttempts++;
                if (failedAttempts >= config.ReplicaFailureThreshold)
                {
                    return false;
                }
    
                await _logHandler.WriteLine($"lowerBoundaryReplicas={config.LowerBoundaryReadyReplicas}, readyReplicas={readyReplicas}. {config.LowerBoundaryReadyReplicas - readyReplicas} ready replica(s) needed for operations", true);
                await Task.Delay(config.ReplicaDelaySeconds.ToMillis());
            }
        }
    }
}