using System;
using System.Diagnostics;

namespace NextPipe.Core
{
    public class RabbitDeploymentConfiguration
    {
        public const string RABBIT_MQ_DEPLOYMENT = "rabbitmq";
        public const string NEXT_PIPE_DEPLOYMENT = "nextpipe-deployment";
        
        // Callback when the infrastructure has been validated
        public int LowerBoundaryReadyReplicas { get; }
        public int ReplicaTrialFailureThreshold { get; }
        public int ReplicaTrialDelaySeconds { get; }
        
        public RabbitDeploymentConfiguration(int lowerBoundaryReadyReplicas, int replicaTrialFailureThreshold, int replicaTrialDelaySeconds)
        {
            LowerBoundaryReadyReplicas = lowerBoundaryReadyReplicas;
            ReplicaTrialFailureThreshold = replicaTrialFailureThreshold;
            ReplicaTrialDelaySeconds = replicaTrialDelaySeconds;
        }
    }
}