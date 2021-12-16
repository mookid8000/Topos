using System;

namespace Topos.Logging.Null;

public class NullLogger : ILogger
{
    public void Debug(string message, params object[] args)
    {
    }

    public void Info(string message, params object[] args)
    {
    }

    public void Warn(string message, params object[] args)
    {
    }

    public void Warn(Exception exception, string message, params object[] args)
    {
    }

    public void Error(string message, params object[] args)
    {
    }

    public void Error(Exception exception, string message, params object[] args)
    {
    }
}