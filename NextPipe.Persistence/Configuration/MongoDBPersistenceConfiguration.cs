namespace NextPipe.Persistence.Configuration
{
    public class MongoDBPersistenceConfiguration
    {
        public string MongoClusterConnectionString { get; set; }
        public string DefaultDatabaseName { get; set; }
    }
}