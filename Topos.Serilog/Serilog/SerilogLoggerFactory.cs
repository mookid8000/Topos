using System;
using Serilog;
using Topos.Logging;
using ILogger = Serilog.ILogger;

namespace Topos.Serilog;

public class SerilogLoggerFactory : ILoggerFactory
{
    readonly ILogger _logger;

    public SerilogLoggerFactory(ILogger logger = null) => _logger = logger ?? Log.Logger;

    public Logging.ILogger GetLogger(Type type) => new SerilogLogger(_logger, type);
}