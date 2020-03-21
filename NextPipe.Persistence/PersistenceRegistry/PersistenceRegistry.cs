using System;
using Lamar;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using NextPipe.Persistence.Configuration;

namespace NextPipe.Persistence.PersistenceRegistry
{
    public class PersistenceRegistry : ServiceRegistry
    {
        public PersistenceRegistry()
        {
            For<IMongoClient>().Use(ctx =>
            {
                Console.WriteLine(ctx.GetInstance<IOptions<MongoDBPersistenceConfiguration>>().Value
                    .MongoClusterConnectionString);
                return new MongoClient(ctx.GetInstance<IOptions<MongoDBPersistenceConfiguration>>().Value
                        .MongoClusterConnectionString);
            }).Singleton();
            
            Scan(scanner =>
            {
                scanner.AssemblyContainingType<PersistenceRegistry>();
                scanner.SingleImplementationsOfInterface();
            });
        }
    }
}