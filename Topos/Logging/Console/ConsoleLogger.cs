using System;
using Topos.Logging.Console;

namespace Topos.Logging
{
    public class ConsoleLogger : ILogger
    {
        readonly StringRenderer _stringRenderer = new StringRenderer();

        public void Debug(string message, params object[] args)
        {
            Write("DBG", message, args);
        }

        public void Info(string message, params object[] args)
        {
            Write("INF", message, args);
        }

        public void Warn(string message, params object[] args)
        {
            Write("WRN", message, args);
        }

        public void Warn(Exception exception, string message, params object[] args)
        {
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