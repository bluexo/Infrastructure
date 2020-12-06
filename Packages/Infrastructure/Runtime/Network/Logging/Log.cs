using System;
using Microsoft.Extensions.Logging;

namespace Mecha
{
    public static class Log
    {
        public static LogLevel LogSeverityLevel = LogLevel.Information;

        public static event EventHandler<LogEventArgs> OnMessage;

        public static void SetLogLevel(LogLevel logLevel)
        {
            LogSeverityLevel = logLevel;
        }

        public static void Debug(object sender, object message, params object[] args)
        {
            LogMessage(sender, LogLevel.Debug, message?.ToString(), args);
        }

        public static void Warn(object sender, object message, params object[] args)
        {
            LogMessage(sender, LogLevel.Warning, message?.ToString(), args);
        }

        public static void Info(object sender, object message, params object[] args)
        {
            LogMessage(sender, LogLevel.Information, message?.ToString(), args);
        }

        public static void Trace(object sender, object message, params object[] args)
        {
            LogMessage(sender, LogLevel.Trace, message?.ToString(), args);
        }

        public static void Error(object sender, object message, params object[] args)
        {
            LogMessage(sender, LogLevel.Error, message?.ToString(), args);
        }

        private static void LogMessage(object sender, LogLevel sev, string format, params object[] args)
        {
            if (OnMessage != null && sev >= LogSeverityLevel)
            {
                var message = (args != null && args.Length > 0) ? string.Format(format ?? string.Empty, args) : format;
                OnMessage.Invoke(sender, new LogEventArgs(sev, message));
            }
        }
    }

}
