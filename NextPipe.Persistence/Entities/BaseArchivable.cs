using MongoDB.Bson.Serialization.Attributes;
using NextPipe.Persistence.Entities.Metadata;
using NextPipe.Persistence.Entities.NextPipeModules;

namespace NextPipe.Persistence.Entities
{
    [BsonKnownTypes(typeof(NextPipeTask), typeof(Module))]
    public abstract class BaseArchivable
    {
        
    }
}