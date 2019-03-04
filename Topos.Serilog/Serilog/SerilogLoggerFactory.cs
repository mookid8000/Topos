using System;
using Serilog;
using Topos.Logging;
using ILogger = Serilog.ILogger;

namespace Topos.Serilog
{
    class SerilogLoggerFactory : ILoggerFactory
    {
        readonly ILogger _logger;

        public SerilogLoggerFactory() : this(Log.Logger)
        {
        }

        public SerilogLoggerFactory(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Logging.ILogger GetLogger(Type type)
        {
            return new SerilogLogger(_logger, type);
        }
    }
}