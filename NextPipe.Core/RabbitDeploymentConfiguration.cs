using System;
using System.Diagnostics;

namespace NextPipe.Core
{
    public class RabbitDeploymentConfiguration
    {
        public const string RABBIT_MQ_DEPLOYMENT = "rabbitmq";
        public const string NEXT_PIPE_DEPLOYMENT = "nextpipe-deployment";
        
        // Callback when the infrastructure has been validated
        public Action SuccessCallback { get; }
        public Action UnsuccessfulCallback { get; }
        public int LowerBoundaryReadyReplicas { get; }
        public int ReplicaTrialFailureThreshold { get; }
        public int ReplicaTrialDelaySeconds { get; }
        
        public RabbitDeploymentConfiguration(Action successCallback, Action unsuccessfulCallback, int lowerBoundaryReadyReplicas, int replicaTrialFailureThreshold, int replicaTrialDelaySeconds)
        {
            SuccessCallback = successCallback;
            UnsuccessfulCallback = unsuccessfulCallback;
            LowerBoundaryReadyReplicas = lowerBoundaryReadyReplicas;
            ReplicaTrialFailureThreshold = replicaTrialFailureThreshold;
            ReplicaTrialDelaySeconds = replicaTrialDelaySeconds;
        }
    }
}