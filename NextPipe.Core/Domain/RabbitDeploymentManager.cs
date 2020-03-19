using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Microsoft.AspNetCore.Http.Features;
using NextPipe.Core.Documents;
using NextPipe.Core.Kubernetes;

namespace NextPipe.Core
{
    public class RabbitDeploymentManager
    {
        private readonly IKubernetes _client;
        private const string RABBIT_MQ_DEPLOYMENT = "rabbitmq";
        private const string NEXT_PIPE_DEPLOYMENT = "nextpipe-deployment";
        private RabbitDeploymentConfiguration _config;
        private readonly IKubectlHelper _kubectlHelper;

        public RabbitDeploymentManager(RabbitDeploymentConfiguration config, IKubectlHelper kubectlHelper)
        {
            _config = config;
            _kubectlHelper = kubectlHelper;
        }

        /// <summary>
        /// Validate and or provision the rabbitMQ infrastructure.  
        /// </summary>
        /// <param name="lowerBoundaryReplicas"></param>
        /// <param name="failureThreshold"></param>
        /// <param name="trialsDelaySec"></param>
        /// <returns></returns>
        public async Task Init(int lowerBoundaryReplicas, int failureThreshold, int trialsDelaySec, bool recursiveCall = false, bool abortOnFailure = false)
        {
            // Run loop until the infrastructure has been provisioned
            var rabbitStatefulSetIsRunning = _kubectlHelper.ValidateStatefulsetIsRunning(RABBIT_MQ_DEPLOYMENT);
            
            Console.WriteLine($"{nameof(RabbitDeploymentManager)}.{nameof(Init)} --> Validating RabbitMQ infrastructure");
        
            if (rabbitStatefulSetIsRunning)
            {
                Console.WriteLine($"RabbitMQ Service deployed --> Checking ready nodes");
                // Validate that at least lowerBoundaryReplicas are running for availability across the cluster
                var isClusterReady = await WaitForLowerBoundaryReplicas(lowerBoundaryReplicas, failureThreshold,
                    trialsDelaySec, RABBIT_MQ_DEPLOYMENT);

                if (isClusterReady)
                {
                    Console.WriteLine("Proceed --> The rabbitMQ cluster has been provisioned and lowerBoundaryReplicasMet=true");
                    // Set up RabbitMQ loadbalancer
                    // Set up NextPipe-ControlPlane loadbalancer
                    // Return succesfull once this completes as finished.
                }
                else
                {
                    Console.WriteLine("Failure --> NextPipe was not able to provision rabbitMQ infrastructure");
                }
            }
            else
            {
                if (abortOnFailure)
                {
                    Console.WriteLine("Failure --> NextPipe failed to setup cluster see logs!");
                }
                // If multiple replicas of NextPipe exist wait for 30 secs to see if one of the other replicas
                // has provisioned the infrastructure. If not initiate helm and provision rabbitMQ infrastructure
                var runningNextPipePods = await _kubectlHelper.GetPodByCustomNameFilter(NEXT_PIPE_DEPLOYMENT, ShellHelper.IdenticalStart);

                if (runningNextPipePods.Count() > 1 && !recursiveCall)
                {
                    // Another NextPipe pod is already running, wait to see if it has taken initiative 
                    await Task.Delay(30.ToMillis());
                    
                    // Call everything again this time provision the infrastructure if it is still not up yet
                    await Init(lowerBoundaryReplicas, failureThreshold, trialsDelaySec, true);
                }
            
                Console.WriteLine("No existing RabbitMQ infrastructure --> Provision RabbitMQ infrastructure");
                var helmManager= new HelmManager();
                helmManager.InstallHelm(true);
                helmManager.InstallRabbitMQ(true);
                await Task.Delay(30.ToMillis());
                // Once helm has installed and rabbitMQ has been provisioned to the cluster by helm retry the init call
                // else abort the process...
                await Init(lowerBoundaryReplicas, failureThreshold, trialsDelaySec, true, true);
            }
        }

        private async Task<bool> WaitForLowerBoundaryReplicas(int lowerBoundaryReplicas, int failureThreshold,
            int trialsDelaySec, string statefulsetname, string nameSpace = "default")
        {
            // true as long as none of the constraints are met
            var failedAttempts = 0;

            var readyReplicas = _kubectlHelper.GetNumberOfStatefulsetReadyReplicas(statefulsetname, nameSpace);
            Console.WriteLine($"lowerBoundaryReplicas={lowerBoundaryReplicas}, readyReplicas={readyReplicas}");

            if (readyReplicas >= lowerBoundaryReplicas)
            {
                return true;
            }
            
            Console.WriteLine("Waiting for ready replicas...");
            
            // Wait the initial delay
            await Task.Delay(trialsDelaySec.ToMillis());
            
            while (true)
            {
                var rReplicas = _kubectlHelper.GetNumberOfStatefulsetReadyReplicas(statefulsetname, nameSpace);
                if (rReplicas >= lowerBoundaryReplicas)
                {
                    return true;
                }
                
                // Increment the failed attempts
                failedAttempts++;
                if (failedAttempts >= failureThreshold)
                {
                    return false;
                }
                Console.WriteLine($"lowerBoundaryReplicas={lowerBoundaryReplicas}, readyReplicas={readyReplicas}. {lowerBoundaryReplicas-readyReplicas} ready replica(s) needed for operations");
                await Task.Delay(trialsDelaySec.ToMillis());
            }
        }
    }
}