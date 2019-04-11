using System;

namespace Topos.Logging.Console
{
    public class ConsoleLoggerFactory : ILoggerFactory
    {
        readonly LogLevel _minimumLogLevel;

        public ConsoleLoggerFactory(LogLevel minimumLogLevel)
        {
            _minimumLogLevel = minimumLogLevel;
        }

        public ILogger GetLogger(Type type)
        {
            return new ConsoleLogger(type, _minimumLogLevel);
        }

        public class ConsoleLogger : ILogger
        {
            readonly StringRenderer _stringRenderer = new StringRenderer();
            readonly LogLevel _minimumLogLevel;
            readonly Type _type;

            public ConsoleLogger(Type type, LogLevel minimumLogLevel)
            {
                _type = type;
                _minimumLogLevel = minimumLogLevel;
            }

            public void Debug(string message, params object[] args)
            {
                if (_minimumLogLevel > LogLevel.Debug) return;

                Write("DBG", message, args);
            }

            public void Info(string message, params object[] args)
            {
                if (_minimumLogLevel > LogLevel.Info) return;
                
                Write("INF", message, args);
            }

            public void Warn(string message, params object[] args)
            {
                if (_minimumLogLevel > LogLevel.Warn) return;

                Write("WRN", message, args);
            }

            public void Warn(Exception exception, string message, params object[] args)
            {
                if (_minimumLogLevel > LogLevel.Warn) return;

                Write("WRN", message, args, exception);
            }

            public void Error(string message, params object[] args)
            {
                Write("ERR", message, args);
            }

            public void Error(Exception exception, string message, params object[] args)
            {
                Write("ERR", message, args, exception);
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
}