using k8s;
using Lamar;

namespace NextPipe.Core.CoreRegistry
{
    public class CoreRegistry : ServiceRegistry
    {
        public CoreRegistry()
        {
            For<IKubernetesClient>().Use<KubernetesClient>();
            For<IKubernetes>().Use(ctx => new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig()));
        }
    }
}