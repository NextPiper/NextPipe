using NextPipe.Core.Domain.SharedValueObjects;
using NextPipe.Core.Helpers;

namespace NextPipe.Core.Domain.NextPipeTask.ValueObject
{
    public class Hostname : BaseValueObject<string>
    {
        public Hostname() : base("cd .. && cd etc && hostname".Bash())
        {
        }
    }
}