using System;
using Confluent.Kafka;
using Serilog.Events;

namespace Topos.Internals
{
    static class KafkaLogLevelExtensions
    {
        public static LogEventLevel ToSerilogLevel(this SyslogLevel level)
        {
            switch (level)
            {
                case SyslogLevel.Emergency:
                    return LogEventLevel.Fatal;
                case SyslogLevel.Alert:
                    return LogEventLevel.Fatal;
                case SyslogLevel.Critical:
                    return LogEventLevel.Fatal;
                case SyslogLevel.Error:
                    return LogEventLevel.Error;
                case SyslogLevel.Warning:
                    return LogEventLevel.Warning;
                case SyslogLevel.Notice:
                    return LogEventLevel.Warning;
                case SyslogLevel.Info:
                    return LogEventLevel.Information;
                case SyslogLevel.Debug:
                    return LogEventLevel.Debug;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }
    }
}