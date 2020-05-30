using MongoDB.Bson.Serialization.Attributes;
using NextPipe.Persistence.Entities.Metadata;

namespace NextPipe.Persistence.Entities
{
    [BsonKnownTypes(typeof(InfrastructureInstallMetadata))]
    public abstract class BaseMetadata
    {
        
    }
}