using System;
using Serilog;

namespace Topos.Serilog;

class SerilogLogger : Logging.ILogger
{
    readonly ILogger _logger;

    public SerilogLogger(ILogger logger, Type type)
    {
        if (logger == null) throw new ArgumentNullException(nameof(logger));
        if (type == null) throw new ArgumentNullException(nameof(type));
        _logger = logger.ForContext(type);
    }

    public void Debug(string message) => _logger.Debug(message);

    public void Debug(string message, object arg1) => _logger.Debug(message, arg1);

    public void Debug(string message, object arg1, object arg2) => _logger.Debug(message, arg1, arg2);

    public void Debug(string message, object arg1, object arg2, object arg3) => _logger.Debug(message, arg1, arg2, arg3);

    public void Info(string message) => _logger.Information(message);

    public void Info(string message, object arg1) => _logger.Information(message, arg1);

    public void Info(string message, object arg1, object arg2) => _logger.Information(message, arg1, arg2);

    public void Info(string message, object arg1, object arg2, object arg3) => _logger.Information(message, arg1, arg2, arg3);

    public void Warn(string message) => _logger.Warning(message);

    public void Warn(string message, object arg1) => _logger.Warning(message, arg1);

    public void Warn(string message, object arg1, object arg2) => _logger.Warning(message, arg1, arg2);

    public void Warn(string message, object arg1, object arg2, object arg3) => _logger.Warning(message, arg1, arg2, arg3);

    public void Warn(Exception exception, string message) => _logger.Warning(exception, message);

    public void Warn(Exception exception, string message, object arg1) => _logger.Warning(exception, message, arg1);

    public void Warn(Exception exception, string message, object arg1, object arg2) => _logger.Warning(exception, message, arg1, arg2);

    public void Warn(Exception exception, string message, object arg1, object arg2, object arg3) => _logger.Warning(exception, message, arg1, arg2, arg3);

    public void Error(string message) => _logger.Error(message);

    public void Error(string message, object arg1) => _logger.Error(message, arg1);

    public void Error(string message, object arg1, object arg2) => _logger.Error(message, arg1, arg2);

    public void Error(string message, object arg1, object arg2, object arg3) => _logger.Error(message, arg1, arg2, arg3);

    public void Error(Exception exception, string message) => _logger.Error(exception, message);

    public void Error(Exception exception, string message, object arg1) => _logger.Error(exception, message, arg1);

    public void Error(Exception exception, string message, object arg1, object arg2) => _logger.Error(exception, message, arg1, arg2);

    public void Error(Exception exception, string message, object arg1, object arg2, object arg3) => _logger.Error(exception, message, arg1, arg2, arg3);
}