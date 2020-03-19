using System;
using System.Diagnostics;
using NextPipe.Core.ValueObjects;

namespace NextPipe.Core
{
    public interface IRabbitDeploymentConfiguration
    {
        int LowerBoundaryReadyReplicas { get; }
        int ReplicaFailureThreshold { get; }
        int ReplicaDelaySeconds { get; }
    }
    
    public class RabbitDeploymentConfiguration : IRabbitDeploymentConfiguration
    {
        private readonly LowerBoundaryReadyReplicas _lowerBoundaryReadyReplicas;
        private readonly ReplicaFailureThreshold _replicaFailureThreshold;
        private readonly ReplicaDelaySeconds _replicaDelaySeconds;


        public RabbitDeploymentConfiguration(LowerBoundaryReadyReplicas lowerBoundaryReadyReplicas, ReplicaFailureThreshold replicaFailureThreshold, ReplicaDelaySeconds replicaDelaySeconds)
        {
            _lowerBoundaryReadyReplicas = lowerBoundaryReadyReplicas;
            _replicaFailureThreshold = replicaFailureThreshold;
            _replicaDelaySeconds = replicaDelaySeconds;
        }

        public int LowerBoundaryReadyReplicas => _lowerBoundaryReadyReplicas.Value;
        public int ReplicaFailureThreshold => _replicaFailureThreshold.Value;
        public int ReplicaDelaySeconds => _replicaDelaySeconds.Value;
    }
}