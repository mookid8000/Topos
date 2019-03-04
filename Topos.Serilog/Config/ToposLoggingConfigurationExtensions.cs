using System;
using Topos.Logging;
using Topos.Serilog;
using ILogger = Serilog.ILogger;

namespace Topos.Config
{
    public static class ToposLoggingConfigurationExtensions
    {
        public static void UseSerilog(this StandardConfigurer<ILoggerFactory> configurer)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
            StandardConfigurer.Open(configurer).Register(c => new SerilogLoggerFactory());
        }

        public static void UseSerilog(this StandardConfigurer<ILoggerFactory> configurer, ILogger logger)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
            StandardConfigurer.Open(configurer).Register(c => new SerilogLoggerFactory(logger));
        }
    }
}