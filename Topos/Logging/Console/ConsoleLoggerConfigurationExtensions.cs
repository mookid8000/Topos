using Topos.Config;

namespace Topos.Logging.Console
{
    public static class ConsoleLoggerConfigurationExtensions
    {
        public static void UseConsole(this StandardConfigurer<ILoggerFactory> configurer)
        {
            StandardConfigurer.Open(configurer).Register(c => new ConsoleLoggerFactory());
        }
    }
}