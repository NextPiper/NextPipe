namespace NextPipe.Persistence.Entities.NextPipeModules
{
    public class ReplicaStatus
    {
        public string DeploymentId { get; set; }
        public bool IsAlive { get; set; }
        public string Status { get; set; } = "";
        public string PodDescribe { get; set; } = "";
        public string PodLog { get; set; } = "";
    }
}