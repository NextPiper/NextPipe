using Lamar;
using NextPipe.Instrumentation;
using Serilog;

namespace NextPipe.Registry
{
    public class LamarRegistry : ServiceRegistry
    {
        public LamarRegistry()
        {
            For<ILogger>().Use(ctx => InstrumentationInitializer.GetInstrumentation().CreateLogger());
        }
    }
}