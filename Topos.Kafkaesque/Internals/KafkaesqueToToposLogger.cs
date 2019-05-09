using System;
using Kafkaesque;

namespace Topos.Internals
{
    class KafkaesqueToToposLogger : ILogger
    {
        readonly Logging.ILogger _logger;

        public KafkaesqueToToposLogger(Logging.ILogger logger) => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        public void Verbose(string message) => _logger.Debug(message);

        public void Verbose(Exception exception, string message) => _logger.Debug($"{message}: {exception}");

        public void Information(string message) => _logger.Info(message);

        public void Information(Exception exception, string message) => _logger.Info($"{message}: {exception}");

        public void Warning(string message) => _logger.Warn(message);

        public void Warning(Exception exception, string message) => _logger.Warn(exception, message);

        public void Error(string message) => _logger.Error(message);

        public void Error(Exception exception, string message) => _logger.Error(exception, message);
    }
}