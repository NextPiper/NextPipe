using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace NextPipe.Instrumentation
{
    public class InstrumentationInitializer
    {
        public static LoggerConfiguration GetInstrumentation()
        {
            return new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .WriteTo.Console(theme: SystemConsoleTheme.Colored);
        }
    }
}