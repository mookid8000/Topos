using System;
// ReSharper disable EmptyGeneralCatchClause

namespace Topos.Logging.Console;

public class ConsoleLoggerFactory : ILoggerFactory
{
    readonly LogLevel _minimumLogLevel;

    public ConsoleLoggerFactory(LogLevel minimumLogLevel) => _minimumLogLevel = minimumLogLevel;

    public ILogger GetLogger(Type type)
    {
        if (type == null) throw new ArgumentNullException(nameof(type));
        return new ConsoleLogger(type, _minimumLogLevel);
    }

    public class ConsoleLogger : ILogger
    {
        readonly StringRenderer _stringRenderer = new();
        readonly LogLevel _minimumLogLevel;
        readonly Type _type;

        public ConsoleLogger(Type type, LogLevel minimumLogLevel)
        {
            _type = type;
            _minimumLogLevel = minimumLogLevel;
        }

        public void Debug(string message)
        {
            if (_minimumLogLevel > LogLevel.Debug) return;
            Write("DBG", message, Array.Empty<object>());
        }

        public void Debug(string message, object arg1)
        {
            if (_minimumLogLevel > LogLevel.Debug) return;
            Write("DBG", message, new[] { arg1 });
        }

        public void Debug(string message, object arg1, object arg2)
        {
            if (_minimumLogLevel > LogLevel.Debug) return;
            Write("DBG", message, new[] { arg1, arg2 });
        }

        public void Debug(string message, object arg1, object arg2, object arg3)
        {
            if (_minimumLogLevel > LogLevel.Debug) return;
            Write("DBG", message, new[] { arg1, arg2, arg3 });
        }

        public void Info(string message)
        {
            if (_minimumLogLevel > LogLevel.Debug) return;
            Write("INF", message, Array.Empty<object>());
        }

        public void Info(string message, object arg1)
        {
            if (_minimumLogLevel > LogLevel.Debug) return;
            Write("INF", message, new[] { arg1 });
        }

        public void Info(string message, object arg1, object arg2)
        {
            if (_minimumLogLevel > LogLevel.Debug) return;
            Write("INF", message, new[] { arg1, arg2 });
        }

        public void Info(string message, object arg1, object arg2, object arg3)
        {
            if (_minimumLogLevel > LogLevel.Debug) return;
            Write("INF", message, new[] { arg1, arg2, arg3 });
        }

        public void Warn(string message)
        {
            if (_minimumLogLevel > LogLevel.Debug) return;
            Write("WRN", message, Array.Empty<object>());
        }

        public void Warn(string message, object arg1)
        {
            if (_minimumLogLevel > LogLevel.Debug) return;
            Write("WRN", message, new[] { arg1 });
        }

        public void Warn(string message, object arg1, object arg2)
        {
            if (_minimumLogLevel > LogLevel.Debug) return;
            Write("WRN", message, new[] { arg1, arg2 });
        }

        public void Warn(string message, object arg1, object arg2, object arg3)
        {
            if (_minimumLogLevel > LogLevel.Debug) return;
            Write("WRN", message, new[] { arg1, arg2, arg3 });
        }

        public void Warn(Exception exception, string message)
        {
            if (_minimumLogLevel > LogLevel.Warn) return;
            Write("WRN", message, Array.Empty<object>(), exception);
        }

        public void Warn(Exception exception, string message, object arg1)
        {
            if (_minimumLogLevel > LogLevel.Warn) return;
            Write("WRN", message, new[] { arg1 }, exception);
        }

        public void Warn(Exception exception, string message, object arg1, object arg2)
        {
            if (_minimumLogLevel > LogLevel.Warn) return;
            Write("WRN", message, new[] { arg1, arg2 }, exception);
        }

        public void Warn(Exception exception, string message, object arg1, object arg2, object arg3)
        {
            if (_minimumLogLevel > LogLevel.Warn) return;
            Write("WRN", message, new[] { arg1, arg2, arg3 }, exception);
        }

        public void Error(string message)
        {
            if (_minimumLogLevel > LogLevel.Debug) return;
            Write("ERR", message, Array.Empty<object>());
        }

        public void Error(string message, object arg1)
        {
            if (_minimumLogLevel > LogLevel.Debug) return;
            Write("ERR", message, new[] { arg1 });
        }

        public void Error(string message, object arg1, object arg2)
        {
            if (_minimumLogLevel > LogLevel.Debug) return;
            Write("ERR", message, new[] { arg1, arg2 });
        }

        public void Error(string message, object arg1, object arg2, object arg3)
        {
            if (_minimumLogLevel > LogLevel.Debug) return;
            Write("ERR", message, new[] { arg1, arg2, arg3 });
        }

        public void Error(Exception exception, string message)
        {
            if (_minimumLogLevel > LogLevel.Warn) return;
            Write("ERR", message, Array.Empty<object>(), exception);
        }

        public void Error(Exception exception, string message, object arg1)
        {
            if (_minimumLogLevel > LogLevel.Warn) return;
            Write("ERR", message, new[] { arg1 }, exception);
        }

        public void Error(Exception exception, string message, object arg1, object arg2)
        {
            if (_minimumLogLevel > LogLevel.Warn) return;
            Write("ERR", message, new[] { arg1, arg2 }, exception);
        }

        public void Error(Exception exception, string message, object arg1, object arg2, object arg3)
        {
            if (_minimumLogLevel > LogLevel.Warn) return;
            Write("ERR", message, new[] { arg1, arg2, arg3 }, exception);
        }

        void Write(string level, string message, object[] args, Exception exception = null)
        {
            var renderedString = _stringRenderer.RenderString(message, args);

            try
            {
                var now = DateTimeOffset.Now;
                var output = exception == null
                    ? $"{now:yyyy-MM-dd} {now:HH:mm:ss.fff zzz} {level} {renderedString}"
                    : $"{now:yyyy-MM-dd} {now:HH:mm:ss.fff zzz} {level} {renderedString}{Environment.NewLine}{exception}";

                System.Console.WriteLine(output);
            }
            catch
            {
            }
        }
    }
}