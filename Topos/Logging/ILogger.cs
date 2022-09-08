using System;

namespace Topos.Logging;

public interface ILogger
{
    void Debug(string message);
    void Debug(string message, object arg1);
    void Debug(string message, object arg1, object arg2);
    void Debug(string message, object arg1, object arg2, object arg3);

    void Info(string message);
    void Info(string message, object arg1);
    void Info(string message, object arg1, object arg2);
    void Info(string message, object arg1, object arg2, object arg3);

    void Warn(string message);
    void Warn(string message, object arg1);
    void Warn(string message, object arg1, object arg2);
    void Warn(string message, object arg1, object arg2, object arg3);

    void Warn(Exception exception, string message);
    void Warn(Exception exception, string message, object arg1);
    void Warn(Exception exception, string message, object arg1, object arg2);
    void Warn(Exception exception, string message, object arg1, object arg2, object arg3);

    void Error(string message);
    void Error(string message, object arg1);
    void Error(string message, object arg1, object arg2);
    void Error(string message, object arg1, object arg2, object arg3);
    
    void Error(Exception exception, string message);
    void Error(Exception exception, string message, object arg1);
    void Error(Exception exception, string message, object arg1, object arg2);
    void Error(Exception exception, string message, object arg1, object arg2, object arg3);
}