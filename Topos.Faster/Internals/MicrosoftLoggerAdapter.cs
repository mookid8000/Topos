using System;
using Microsoft.Extensions.Logging;
using Topos.Logging;

namespace Topos.Internals;

class MicrosoftLoggerAdapter : ILogger
{
    readonly Logging.ILogger _logger;

    public MicrosoftLoggerAdapter(Logging.ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
    {
        var message = $"FASTER {formatter(state, exception)}";

        switch (logLevel)
        {
            case LogLevel.None:
            case LogLevel.Trace:
            case LogLevel.Debug:
                _logger.Debug(message);
                break;
            
            case LogLevel.Information:
                _logger.Info(message);
                break;
            
            case LogLevel.Warning:
                _logger.Warn(message);
                break;
            
            case LogLevel.Error:
            case LogLevel.Critical:
                _logger.Error(message);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
        }
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable BeginScope<TState>(TState state) => null;
}