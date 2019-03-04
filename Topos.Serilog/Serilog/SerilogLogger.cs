using System;
using Serilog;

namespace Topos.Serilog
{
    class SerilogLogger : Logging.ILogger
    {
        readonly ILogger _logger;

        public SerilogLogger(ILogger logger, Type type)
        {
            if (logger == null) throw new ArgumentNullException(nameof(logger));
            if (type == null) throw new ArgumentNullException(nameof(type));
            _logger = logger.ForContext(type);
        }

        public void Debug(string message, params object[] args)
        {
            _logger.Debug(message, args);
        }

        public void Info(string message, params object[] args)
        {
            _logger.Information(message, args);
        }

        public void Warn(string message, params object[] args)
        {
            _logger.Warning(message, args);
        }

        public void Warn(Exception exception, string message, params object[] args)
        {
            _logger.Warning(exception, message, args);
        }

        public void Error(string message, params object[] args)
        {
            _logger.Error(message, args);
        }

        public void Error(Exception exception, string message, params object[] args)
        {
            _logger.Error(exception, message, args);
        }
    }
}