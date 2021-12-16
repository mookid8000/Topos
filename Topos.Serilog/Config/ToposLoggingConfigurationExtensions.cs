using System;
using Topos.Logging;
using Topos.Serilog;
using SerilogLogger = Serilog.ILogger;

namespace Topos.Config;

public static class ToposLoggingConfigurationExtensions
{
    public static void UseSerilog(this StandardConfigurer<ILoggerFactory> configurer)
    {
        if (configurer == null) throw new ArgumentNullException(nameof(configurer));
        StandardConfigurer.Open(configurer).Register(_ => new SerilogLoggerFactory());
    }

    public static void UseSerilog(this StandardConfigurer<ILoggerFactory> configurer, SerilogLogger logger)
    {
        if (configurer == null) throw new ArgumentNullException(nameof(configurer));
        StandardConfigurer.Open(configurer).Register(_ => new SerilogLoggerFactory(logger));
    }
}