using System;
using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Core.ValueObjects;
using NextPipe.Utilities.Documents.Responses;
using SimpleSoft.Mediator;

namespace NextPipe.Core.Commands.Commands.StartupCommands
{
    public class RequestInitializeInfrastructure : Command<TaskRequestResponse>
    {
        public LowerBoundaryReadyReplicas LowerBoundaryReadyReplicas { get; }
        public ReplicaFailureThreshold ReplicaFailureThreshold { get; }
        public ReplicaDelaySeconds ReplicaDelaySeconds { get; }
        
        public RabbitNumberOfReplicas RabbitNumberOfReplicas { get; }

        public RequestInitializeInfrastructure(int lowerBoundaryReadyReplicas = 2, int replicaFailureThreshold = 6, int replicaDelaySeconds = 30, int numberOfReplicas = 3)
        {
            LowerBoundaryReadyReplicas = new LowerBoundaryReadyReplicas(lowerBoundaryReadyReplicas);
            ReplicaFailureThreshold = new ReplicaFailureThreshold(replicaFailureThreshold);
            ReplicaDelaySeconds = new ReplicaDelaySeconds(replicaDelaySeconds);
            RabbitNumberOfReplicas = new RabbitNumberOfReplicas(numberOfReplicas);
        }
    }
}