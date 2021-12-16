using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Topos.Logging;
using Topos.Logging.Console;

namespace Topos.Tests.Contracts.Stubs;

class ListLoggerFactory : ILoggerFactory, IEnumerable<ListLoggerFactory.LogLine>
{
    readonly ConcurrentQueue<LogLine> _lines = new ConcurrentQueue<LogLine>();
    readonly StringRenderer _stringRenderer = new StringRenderer();
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

        public void Debug(string message, params object[] args) => Append(LogLevel.Debug, message, args);

        public void Info(string message, params object[] args) => Append(LogLevel.Info, message, args);

        public void Warn(string message, params object[] args) => Append(LogLevel.Warn, message, args);

        public void Warn(Exception exception, string message, params object[] args) => Append(LogLevel.Warn, message, args, exception);

        public void Error(string message, params object[] args) => Append(LogLevel.Error, message, args);

        public void Error(Exception exception, string message, params object[] args) => Append(LogLevel.Error, message, args, exception);

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