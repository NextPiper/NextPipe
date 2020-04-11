using System;
using System.Collections.Generic;

namespace NextPipe.Persistence.Entities.NextPipeModules
{
    public class Module : BaseArchivable, IEntity
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime EditedAt { get; set; } = DateTime.Now;
        
        public string ImageName { get; set; }
        public string ModuleName { get; set; }
        public ModuleStatus ModuleStatus { get; set; }
        public int DesiredReplicas { get; set; }
        public int CurrentReadyReplicas { get; set; } = 0;
        public List<ReplicaStatus> ReplicaLogs { get; set; } = new List<ReplicaStatus>();
        public LoadBalancerConfig LoadBalancerConfig { get; set; } = new LoadBalancerConfig {NeedLoadBalancer = false, Port = 0, TargetPort = 0};
        public string Logs { get; set; } = "";
        public LoadBalancer LoadBalancer { get; set; }

    }
}