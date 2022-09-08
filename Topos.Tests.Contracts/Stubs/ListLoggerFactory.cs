using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Topos.Logging;
using Topos.Logging.Console;

namespace Topos.Tests.Contracts.Stubs;

class ListLoggerFactory : ILoggerFactory, IEnumerable<ListLoggerFactory.LogLine>
{
    readonly ConcurrentQueue<LogLine> _lines = new();
    readonly StringRenderer _stringRenderer = new();
    readonly bool _outputToConsole;

    public ListLoggerFactory(bool outputToConsole = false)
    {
        _outputToConsole = outputToConsole;
    }

    public ILogger GetLogger(Type type) => new Logger(type, _lines, _stringRenderer, _outputToConsole);

    public void DumpLogs() => Console.WriteLine(string.Join(Environment.NewLine, _lines));

    class Logger : ILogger
    {
        readonly ConcurrentQueue<LogLine> _lines;
        readonly StringRenderer _stringRenderer;
        readonly bool _outputToConsole;
        readonly Type _type;

        public Logger(Type type, ConcurrentQueue<LogLine> lines, StringRenderer stringRenderer, bool outputToConsole)
        {
            _type = type ?? throw new ArgumentNullException(nameof(type));
            _lines = lines ?? throw new ArgumentNullException(nameof(lines));
            _stringRenderer = stringRenderer ?? throw new ArgumentNullException(nameof(stringRenderer));
            _outputToConsole = outputToConsole;
        }

        public void Debug(string message) => Append(LogLevel.Debug, message, Array.Empty<object>());
        public void Debug(string message, object arg1) => Append(LogLevel.Debug, message, new[] { arg1 });
        public void Debug(string message, object arg1, object arg2) => Append(LogLevel.Debug, message, new[] { arg1, arg2 });
        public void Debug(string message, object arg1, object arg2, object arg3) => Append(LogLevel.Debug, message, new[] { arg1, arg2, arg3 });
        
        public void Info(string message) => Append(LogLevel.Info, message, Array.Empty<object>());
        public void Info(string message, object arg1) => Append(LogLevel.Info, message, new[] { arg1 });
        public void Info(string message, object arg1, object arg2) => Append(LogLevel.Info, message, new[] { arg1, arg2 });
        public void Info(string message, object arg1, object arg2, object arg3) => Append(LogLevel.Info, message, new[] { arg1, arg2, arg3 });

        public void Warn(string message) => Append(LogLevel.Warn, message, Array.Empty<object>());
        public void Warn(string message, object arg1) => Append(LogLevel.Warn, message, new[] { arg1 });
        public void Warn(string message, object arg1, object arg2) => Append(LogLevel.Warn, message, new[] { arg1, arg2 });
        public void Warn(string message, object arg1, object arg2, object arg3) => Append(LogLevel.Warn, message, new[] { arg1, arg2, arg3 });

        public void Warn(Exception exception, string message) => Append(LogLevel.Warn, message, Array.Empty<object>(), exception);
        public void Warn(Exception exception, string message, object arg1) => Append(LogLevel.Warn, message, new[] { arg1 }, exception);
        public void Warn(Exception exception, string message, object arg1, object arg2) => Append(LogLevel.Warn, message, new[] { arg1, arg2 }, exception);
        public void Warn(Exception exception, string message, object arg1, object arg2, object arg3) => Append(LogLevel.Warn, message, new[] { arg1, arg2, arg3 }, exception);

        public void Error(string message) => Append(LogLevel.Error, message, Array.Empty<object>());
        public void Error(string message, object arg1) => Append(LogLevel.Error, message, new[] { arg1 });
        public void Error(string message, object arg1, object arg2) => Append(LogLevel.Error, message, new[] { arg1, arg2 });
        public void Error(string message, object arg1, object arg2, object arg3) => Append(LogLevel.Error, message, new[] { arg1, arg2, arg3 });

        public void Error(Exception exception, string message) => Append(LogLevel.Error, message, Array.Empty<object>(), exception);
        public void Error(Exception exception, string message, object arg1) => Append(LogLevel.Error, message, new[] { arg1 }, exception);
        public void Error(Exception exception, string message, object arg1, object arg2) => Append(LogLevel.Error, message, new[] { arg1, arg2 }, exception);
        public void Error(Exception exception, string message, object arg1, object arg2, object arg3) => Append(LogLevel.Error, message, new[] { arg1, arg2, arg3 }, exception);
        
        void Append(LogLevel level, string message, object[] args, Exception exception = null)
        {
            var time = DateTimeOffset.Now;
            var text = _stringRenderer.RenderString(message, args);
            var line = new LogLine(time, _type, level, text, exception);

            _lines.Enqueue(line);

            if (_outputToConsole)
            {
                Console.WriteLine(line);
            }
        }
    }

    public IEnumerator<LogLine> GetEnumerator() => _lines.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public class LogLine
    {
        public DateTimeOffset Time { get; }
        public Type Context { get; }
        public LogLevel Level { get; }
        public string Text { get; }
        public Exception Exception { get; }

        public LogLine(DateTimeOffset time, Type context, LogLevel level, string text, Exception exception)
        {
            Time = time;
            Context = context;
            Level = level;
            Text = text;
            Exception = exception;
        }

        public override string ToString() => Exception == null

            ? $"{Time:HH:mm:ss}|{Level}|{Context.Name}|{Text}"

            : $@"{Time:HH:mm:ss}|{Level}|{Context.Name}|{Text}
{Exception}";
    }
}