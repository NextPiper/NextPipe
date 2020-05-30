using System.Collections.Generic;
using k8s.Models;

namespace NextPipe.Core.Domain.Module.KubernetesModule
{
    public class KubernetesModule
    {
        public V1Deployment Deployment { get; }
        public IEnumerable<V1Pod> Pods { get; }

        public KubernetesModule(V1Deployment deployment, IEnumerable<V1Pod> pods)
        {
            Deployment = deployment;
            Pods = pods;
        }
    }
}