using Topos.Config;

namespace Topos.Logging.Console;

public static class ConsoleLoggerConfigurationExtensions
{
    public static void UseConsole(this StandardConfigurer<ILoggerFactory> configurer, LogLevel minimumLogLevel = LogLevel.Debug)
    {
        StandardConfigurer.Open(configurer).Register(c => new ConsoleLoggerFactory(minimumLogLevel));
    }
}