using Lamar;
using NextPipe.Messaging.Infrastructure.Contracts;
using NextPipe.Messaging.Infrastructure.Factories;
using NextPipe.Messaging.Infrastructure.Mediators;
using SimpleSoft.Mediator;

namespace NextPipe.Messaging.Infrastructure.Registry
{
    public class MessagingInfrastructureRegistry : ServiceRegistry
    {
        public MessagingInfrastructureRegistry()
        {
            // Mediator setup
            For<IMediator>().Use<Mediator>();
            For<IMediatorFactory>().Use<LamarMediatorFactory>();
            
            // Register the different routers
            For<ICommandRouter>().Use<MediatorCommandRouter>();
            For<IQueryRouter>().Use<MediatorQueryRouter>();
            For<IEventPublisher>().Use<MediatorEventPublisher>();


        }
    }
}