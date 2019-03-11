using Serilog;
using Serilog.Core;
using Serilog.Events;
using Testy;
using Testy.General;

namespace Topos.Tests
{
    public abstract class ToposFixtureBase : FixtureBase
    {
        static ToposFixtureBase()
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.ControlledBy(LogLevelSwitch)
                .CreateLogger();
        }

        static LoggingLevelSwitch LogLevelSwitch { get; } = new LoggingLevelSwitch(LogEventLevel.Verbose);

        protected void SetLogLevelTo(LogEventLevel level)
        {
            LogLevelSwitch.MinimumLevel = level;

            Using(new DisposableCallback(() => LogLevelSwitch.MinimumLevel = LogEventLevel.Verbose));
        }

        protected ILogger Logger => Log.ForContext("SourceContext", GetType());
    }
}