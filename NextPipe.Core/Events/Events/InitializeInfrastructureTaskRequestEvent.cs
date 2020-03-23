using System;
using System.Security.Cryptography.X509Certificates;
using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Core.ValueObjects;

namespace NextPipe.Core.Events.Events
{
    public class InitializeInfrastructureTaskRequestEvent : BaseEvent
    {
        public Id TaskId { get; }
        public LowerBoundaryReadyReplicas LowerBoundaryReadyReplicas { get; }
        public ReplicaFailureThreshold ReplicaFailureThreshold { get; }
        public ReplicaDelaySeconds ReplicaDelaySeconds { get; }
        public RabbitNumberOfReplicas RabbitNumberOfReplicas { get; }


        public InitializeInfrastructureTaskRequestEvent(Id taskId, LowerBoundaryReadyReplicas lowerBoundaryReadyReplicas, ReplicaFailureThreshold replicaFailureThreshold, ReplicaDelaySeconds replicaDelaySeconds, RabbitNumberOfReplicas rabbitNumberOfReplicas)
        {
            TaskId = taskId;
            LowerBoundaryReadyReplicas = lowerBoundaryReadyReplicas;
            ReplicaFailureThreshold = replicaFailureThreshold;
            ReplicaDelaySeconds = replicaDelaySeconds;
            RabbitNumberOfReplicas = rabbitNumberOfReplicas;
        }
    }
}