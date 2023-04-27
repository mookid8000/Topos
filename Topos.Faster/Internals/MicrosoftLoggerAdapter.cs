using System;
using Microsoft.Extensions.Logging;

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
        switch (logLevel)
        {
            case LogLevel.None:
            case LogLevel.Trace:
            case LogLevel.Debug:
                _logger.Debug(formatter(state, exception));
                break;
            
            case LogLevel.Information:
                _logger.Info(formatter(state, exception));
                break;
            
            case LogLevel.Warning:
                _logger.Warn(formatter(state, exception));
                break;
            
            case LogLevel.Error:
            case LogLevel.Critical:
                _logger.Error(formatter(state, exception));
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
        }
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable BeginScope<TState>(TState state) => null;
}