using System;
using System.IO;
using System.Linq;
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
            var filePath = GetNextFilePath();

            Console.WriteLine($"Writing logs to {filePath}");

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(filePath, flushToDiskInterval: TimeSpan.FromMilliseconds(0.01))
                .MinimumLevel.ControlledBy(LogLevelSwitch)
                .CreateLogger();
        }

        static string GetNextFilePath() =>
            Enumerable.Range(1, int.MaxValue)
                .Select(n => Path.Combine($@"C:\logs\topos-tests\logs-{n}.txt"))
                .First(filePath => !File.Exists(filePath));

        static LoggingLevelSwitch LogLevelSwitch { get; } = new LoggingLevelSwitch(LogEventLevel.Verbose);

        protected void SetLogLevelTo(LogEventLevel level)
        {
            LogLevelSwitch.MinimumLevel = level;

            Using(new DisposableCallback(() => LogLevelSwitch.MinimumLevel = LogEventLevel.Verbose));
        }

        protected ILogger Logger => Log.ForContext("SourceContext", GetType());
    }
}