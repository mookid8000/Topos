using System;

namespace Topos.Logging.Null;

public class NullLogger : ILogger
{
    public void Debug(string message)
    {
    }

    public void Debug(string message, object arg1)
    {
    }

    public void Debug(string message, object arg1, object arg2)
    {
    }

    public void Debug(string message, object arg1, object arg2, object arg3)
    {
    }

    public void Info(string message)
    {
    }

    public void Info(string message, object arg1)
    {
    }

    public void Info(string message, object arg1, object arg2)
    {
    }

    public void Info(string message, object arg1, object arg2, object arg3)
    {
    }

    public void Warn(string message)
    {
    }

    public void Warn(string message, object arg1)
    {
    }

    public void Warn(string message, object arg1, object arg2)
    {
    }

    public void Warn(string message, object arg1, object arg2, object arg3)
    {
    }

    public void Warn(Exception exception, string message)
    {
    }

    public void Warn(Exception exception, string message, object arg1)
    {
    }

    public void Warn(Exception exception, string message, object arg1, object arg2)
    {
    }

    public void Warn(Exception exception, string message, object arg1, object arg2, object arg3)
    {
    }

    public void Error(string message)
    {
    }

    public void Error(string message, object arg1)
    {
    }

    public void Error(string message, object arg1, object arg2)
    {
    }

    public void Error(string message, object arg1, object arg2, object arg3)
    {
    }

    public void Error(Exception exception, string message)
    {
    }

    public void Error(Exception exception, string message, object arg1)
    {
    }

    public void Error(Exception exception, string message, object arg1, object arg2)
    {
    }

    public void Error(Exception exception, string message, object arg1, object arg2, object arg3)
    {
    }
}