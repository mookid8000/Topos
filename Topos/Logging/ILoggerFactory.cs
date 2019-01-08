using System;

namespace Topos.Logging
{
    public interface ILoggerFactory
    {
        ILogger GetLogger(Type type);
    }
}