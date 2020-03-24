namespace NextPipe.Persistence.Entities.Metadata
{
    public class InfrastructureInstallMetadata : BaseMetadata
    {
        public int LowerBoundaryReadyReplicas { get; set; }
        public int ReplicaFailureThreshold { get; set; }
        public int ReplicaDelaySeconds { get; set; }
        public int RabbitNumberOfReplicas { get; set; }
    }
}