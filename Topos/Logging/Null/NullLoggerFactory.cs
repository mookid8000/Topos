using System;

namespace Topos.Logging.Null
{
    public class NullLoggerFactory : ILoggerFactory
    {
        public ILogger GetLogger(Type type) => new NullLogger();
    }
}