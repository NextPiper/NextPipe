using Lamar;
using NextPipe.Core.CoreRegistry;
using NextPipe.Instrumentation;
using Serilog;

namespace NextPipe.Registry
{
    public class LamarRegistry : ServiceRegistry
    {
        public LamarRegistry()
        {
            For<ILogger>().Use(ctx => InstrumentationInitializer.GetInstrumentation().CreateLogger());
            IncludeRegistry<CoreRegistry>();
        }
    }
}