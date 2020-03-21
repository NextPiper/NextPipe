using k8s;
using Lamar;
using NextPipe.Core.Kubernetes;
using NextPipe.Messaging.Infrastructure.Registry;
using NextPipe.Persistence.PersistenceRegistry;
using SimpleSoft.Mediator;

namespace NextPipe.Core.CoreRegistry
{
    public class CoreRegistry : ServiceRegistry
    {
        public CoreRegistry()
        {
            For<IKubernetes>().Use(ctx => new k8s.Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig()));
            For<IKubectlHelper>().Use<KubectlHelper>();
            For<IRabbitDeploymentManager>().Use<RabbitDeploymentManager>();
            For<IHelmManager>().Use<HelmManager>();
            
            IncludeRegistry<MessagingInfrastructureRegistry>();
            IncludeRegistry<PersistenceRegistry>();
            
            Scan(scanner =>
            {
                scanner.AssemblyContainingType<CoreRegistry>();
                
                scanner.ConnectImplementationsToTypesClosing(typeof(ICommandHandler<>));
                scanner.ConnectImplementationsToTypesClosing(typeof(ICommandHandler<,>));
                scanner.ConnectImplementationsToTypesClosing(typeof(IQueryHandler<,>));
                scanner.ConnectImplementationsToTypesClosing(typeof(IEventHandler<>));
            });
        }
    }
}