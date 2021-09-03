using Microsoft.Extensions.Logging;
using System;

namespace Love
{
    public class LogEventArgs 
    {
        public LogLevel LogSeverity { get; }

        public string Message { get; }

        public LogEventArgs(LogLevel logSeverity, string message)
        {
            LogSeverity = logSeverity;
            Message = message;
        }
    }
}