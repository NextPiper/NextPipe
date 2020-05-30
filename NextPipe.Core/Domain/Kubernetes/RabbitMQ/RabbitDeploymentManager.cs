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
using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Core.Helpers;
using NextPipe.Core.Kubernetes;
using NextPipe.Utilities.Resources;

namespace NextPipe.Core
{
    public interface IRabbitDeploymentManager
    {
        bool IsInfrastructureRunning(int lowerBoundaryReadyReplicas);
        Task Deploy(IRabbitDeploymentManagerConfiguration config);
        Task Cleanup(Id taskId, Func<Id, ILogHandler, Task> successHandler, Func<Id, ILogHandler, Task> failureHandler, bool verboseLogging = false);
        void AttachTaskIdAndUpdateHandler(Id taskId, Func<Id, ILogHandler, Task> updateHandler);
        void AttachPreviousLogs(string logs);
        void SetVerboseLogging(bool verboseLogging);
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
        private bool verboseLogging;

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
            AttachTaskIdAndUpdateHandler(config.TaskId, config.UpdateCallback);
            await Deploy(config, false);
        }

        public void AttachTaskIdAndUpdateHandler(Id taskId, Func<Id, ILogHandler, Task> updateHandler)
        {
            _logHandler.AttachTaskIdAndUpdateHandler(taskId, updateHandler);
        }

        public void AttachPreviousLogs(string logs)
        {
            _logHandler.AttachPreviousLogs(logs);
        }

        public void SetVerboseLogging(bool verboseLogging)
        {
            this.verboseLogging = verboseLogging;
        }

        private async Task Deploy(IRabbitDeploymentManagerConfiguration config, bool abortOnFailure = false)
        {
            // Check if statefulset is already deployed
            await _logHandler.WriteCmd($"{nameof(RabbitDeploymentManager)}.{nameof(Deploy)}", verboseLogging);
            await _logHandler.WriteLine("Validating statefulset is running...", verboseLogging);
            var rabbitStatefulSetIsRunning = _kubectlHelper.ValidateStatefulsetIsRunning(RABBIT_MQ_STATEFULSET);
            
            if (rabbitStatefulSetIsRunning)
            {
                await _logHandler.WriteLine($"Rabbit Statefulset is running -->", verboseLogging);
                await ValidateRabbitMQDeployment(config);
            }
            else
            {
                if (abortOnFailure)
                {
                    await _logHandler.WriteLine("Failed to install rabbitMQ infrastructure", verboseLogging);
                    await Cleanup();
                    await _logHandler.WriteLine($"CleanUp Complete --> config.FailureCallback()", verboseLogging);
                    await config.FailureCallback(config.TaskId, _logHandler);
                    return;
                }

                await InstallHelmAndRabbitMQ(config);
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
            await _logHandler.WriteLine($"rabbitConfig.IsServiceEnabled={_rabbitConfig.Value.IsRabbitServiceEnabled}",
                verboseLogging);
            if (_rabbitConfig.Value.IsRabbitServiceEnabled)
            {
                await _logHandler.WriteLine("Installing RabbitMQ Load Balancer Service", verboseLogging);
                await _kubectlHelper.InstallService(KubectlHelper.GetRabbitMQService());
            }
        }
        
        public async Task Cleanup(Id taskId, Func<Id, ILogHandler, Task> successHandler, Func<Id, ILogHandler, Task> failureHandler, bool verboseLogging = false)
        {
            await _logHandler.WriteCmd($"{nameof(RabbitDeploymentManager)}.{nameof(Cleanup)}", verboseLogging);
            
            // Uninstall helm
            await _helmManager.CleanUp(RABBIT_MQ_STATEFULSET, _logHandler, verboseLogging);
            
            // Uninstall rabbitmq service
            await _logHandler.WriteCmd($"{nameof(_kubectlHelper)}.{nameof(KubectlHelper.DeleteService)}({RABBIT_MQ_SERVICE})",verboseLogging);
            var result = await _kubectlHelper.DeleteService(RABBIT_MQ_SERVICE);
            await _logHandler.WriteLine(result, verboseLogging);
            
            // Clean-up pvc!
            await _logHandler.WriteCmd($"{nameof(KubectlHelper)}.{nameof(KubectlHelper.DeletePVCList)}", verboseLogging);
            await _logHandler.WriteLine("Cleaning Persistent Volume Claims from rabbitMQ", verboseLogging);
            var pvcList = await _kubectlHelper.GetPVCsByCustomerNameFilter(RABBIT_MQ_PVC, ShellHelper.IdenticalStart);
            foreach (var pvc in pvcList)
            {
                await _logHandler.WriteLine($"Deleting PVC: {pvc.Metadata.Name}", verboseLogging);
            }
            await _kubectlHelper.DeletePVCList(pvcList);
            await _logHandler.WriteLine($"Successful cleanup --> Calling successHandler");
            await successHandler(taskId, _logHandler);
        }

        private async Task Cleanup()
        {
            await _logHandler.WriteCmd($"{nameof(RabbitDeploymentManager)}.{nameof(Cleanup)}", verboseLogging);
            
            // Uninstall helm
            await _helmManager.CleanUp(RABBIT_MQ_STATEFULSET, _logHandler, verboseLogging);
            
            // Uninstall rabbitmq service
            await _logHandler.WriteCmd($"{nameof(_kubectlHelper)}.{nameof(KubectlHelper.DeleteService)}({RABBIT_MQ_SERVICE})",verboseLogging);
            var result = await _kubectlHelper.DeleteService(RABBIT_MQ_SERVICE);
            await _logHandler.WriteLine(result, verboseLogging);
            
            // Clean-up pvc!
            await _logHandler.WriteCmd($"{nameof(KubectlHelper)}.{nameof(KubectlHelper.DeletePVCList)}", verboseLogging);
            await _logHandler.WriteLine("Cleaning Persistent Volume Claims from rabbitMQ", verboseLogging);
            var pvcList = await _kubectlHelper.GetPVCsByCustomerNameFilter(RABBIT_MQ_PVC, ShellHelper.IdenticalStart);
            foreach (var pvc in pvcList)
            {
                await _logHandler.WriteLine($"Deleting PVC: {pvc.Metadata.Name}", verboseLogging);
            }
            await _kubectlHelper.DeletePVCList(pvcList);
        }

        private async Task InstallHelmAndRabbitMQ(IRabbitDeploymentManagerConfiguration config)
        {
            await _logHandler.WriteLine("No existing RabbitMQ infrastructure --> Provision RabbitMQ infrastructure", verboseLogging);
            await _helmManager.InstallHelm(_logHandler, verboseLogging);
            await _helmManager.InstallRabbitMQ(RABBIT_MQ_STATEFULSET, _logHandler,  verboseLogging, config.RabbitNumberOfReplicas);
            await Task.Delay(30.SecToMillis());
        }

        private async Task ValidateRabbitMQDeployment(IRabbitDeploymentManagerConfiguration config)
        {
            await _logHandler.WriteCmd($"{nameof(RabbitDeploymentManager)}.{nameof(ValidateRabbitMQDeployment)}", verboseLogging);
            await _logHandler.WriteLine("Waiting for lowerboundary replicas to come online", verboseLogging);
            // Validate that at least lowerBoundaryReplicas are running for availability across the cluster
            var isClusterReady = await WaitForLowerBoundaryReplicas(config, RABBIT_MQ_STATEFULSET);
            if (isClusterReady)
            {
                await _logHandler.WriteLine("RabbitMQ cluster is running with desired lowerBoundary replicas online -->", verboseLogging);
                await CreateRabbitMQService();
                await _logHandler.WriteLine("RabbitMQ successfully installed with desired configuration", verboseLogging);
                await _logHandler.WriteCmd($"{nameof(config.SuccessCallback)}()", verboseLogging);
                await config.SuccessCallback(config.TaskId, _logHandler);
            }
            else
            {
                await _logHandler.WriteLine("RabbitMQ failed to get lowerBoundary replicas running", verboseLogging);
                await Cleanup();
                await _logHandler.WriteLine($"CleanUp Complete --> config.FailureCallback()", verboseLogging);
                await config.FailureCallback(config.TaskId, _logHandler);
            }
        }
        
        private async Task<bool> WaitForLowerBoundaryReplicas(IRabbitDeploymentManagerConfiguration config, string statefulSetname, string nameSpace = "default")
        {
            // true as long as none of the constraints are met
            var failedAttempts = 0;
    
            var readyReplicas = _kubectlHelper.GetNumberOfStatefulsetReadyReplicas(statefulSetname, nameSpace);
            await _logHandler.WriteLine($"lowerBoundaryReplicas={config.LowerBoundaryReadyReplicas}, readyReplicas={readyReplicas}. {config.LowerBoundaryReadyReplicas - readyReplicas} ready replica(s) needed for operations. Attempt {failedAttempts}/{config.ReplicaFailureThreshold}",
                verboseLogging, LogHandler.InProgressTemplate);

            if (readyReplicas >= config.LowerBoundaryReadyReplicas)
            {
                return true;
            }
    
            await _logHandler.WriteLine($"Waiting for ready replicas... {config.ReplicaDelaySeconds}", verboseLogging);
    
            // Wait the initial delay
            await Task.Delay(config.ReplicaDelaySeconds.SecToMillis());
    
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
    
                await _logHandler.WriteLine($"lowerBoundaryReplicas={config.LowerBoundaryReadyReplicas}, readyReplicas={readyReplicas}. {config.LowerBoundaryReadyReplicas - readyReplicas} ready replica(s) needed for operations. Attempt {failedAttempts}/{config.ReplicaFailureThreshold}",
                    verboseLogging, LogHandler.InProgressTemplate);
                await Task.Delay(config.ReplicaDelaySeconds.SecToMillis());
            }
        }
    }
}