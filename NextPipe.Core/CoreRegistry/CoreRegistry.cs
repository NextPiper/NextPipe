using Lamar;

namespace NextPipe.Core.CoreRegistry
{
    public class CoreRegistry : ServiceRegistry
    {
        public CoreRegistry()
        {
            For<IKubernetesClient>().Use<KubernetesClient>();
        }
    }
}