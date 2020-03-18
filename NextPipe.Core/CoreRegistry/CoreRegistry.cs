using k8s;
using Lamar;
using NextPipe.Messaging.Infrastructure.Registry;
using NextPipe.Persistence.PersistenceRegistry;
using SimpleSoft.Mediator;

namespace NextPipe.Core.CoreRegistry
{
    public class CoreRegistry : ServiceRegistry
    {
        public CoreRegistry()
        {
            For<IKubernetesClient>().Use<KubernetesClient>();
            For<IKubernetes>().Use(ctx => new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig()));
            
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