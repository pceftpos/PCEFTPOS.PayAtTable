using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using Common.Logging;

namespace PayAtTable.API.Helpers
{
    public class TraceRecord
    {
        public TraceRecord()
        {
            Message = "";
            Data = null;
            Level = LogLevel.Off;
        }

        public void Set(string message)
        {
            Message = message;
        }

        public void Set(string message, object data)
        {
            Message = message;
            Data = data;
        }

        public void Set(string message, object data, Exception exception)
        {
            Message = message;
            Data = data;
            Exception = exception;
        }

        public void Set(string message, Exception exception)
        {
            Message = message;
            Exception = exception;
        }

        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
        public Exception Exception { get; set; }
    }
    
    public static class CommonLoggingExtensions
    {
        public static void TraceEx(this Common.Logging.ILog log, Action<TraceRecord> traceAction, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            LogEx(log, LogLevel.Trace, traceAction, member, line);
        }

        public static void TraceEx(this Common.Logging.ILog log, String message, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            LogEx(log, LogLevel.Trace, ta => { ta.Message = message; }, member, line);
        }

        public static void DebugEx(this Common.Logging.ILog log, Action<TraceRecord> traceAction, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            LogEx(log, LogLevel.Debug, traceAction, member, line);
        }

        public static void DebugEx(this Common.Logging.ILog log, String message, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            LogEx(log, LogLevel.Debug, ta => { ta.Message = message; }, member, line);
        }

        public static void InfoEx(this Common.Logging.ILog log, Action<TraceRecord> traceAction, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            LogEx(log, LogLevel.Info, traceAction, member, line);
        }

        public static void InfoEx(this Common.Logging.ILog log, String message, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            LogEx(log, LogLevel.Info, ta => { ta.Message = message; }, member, line);
        }

        public static void WarnEx(this Common.Logging.ILog log, Action<TraceRecord> traceAction, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            LogEx(log, LogLevel.Warn, traceAction, member, line);
        }

        public static void WarnEx(this Common.Logging.ILog log, String message, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            LogEx(log, LogLevel.Warn, ta => { ta.Message = message; }, member, line);
        }

        public static void ErrorEx(this Common.Logging.ILog log, Action<TraceRecord> traceAction, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            LogEx(log, LogLevel.Error, traceAction, member, line);
        }

        public static void ErrorEx(this Common.Logging.ILog log, String message, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            LogEx(log, LogLevel.Error, ta => { ta.Message = message; }, member, line);
        }

        public static void FatalEx(this Common.Logging.ILog log, Action<TraceRecord> traceAction, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            LogEx(log, LogLevel.Fatal, traceAction, member, line);
        }

        public static void FatalEx(this Common.Logging.ILog log, String message, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            LogEx(log, LogLevel.Fatal, ta => { ta.Message = message; }, member, line);
        }

        public static void LogEx(this Common.Logging.ILog log, Common.Logging.LogLevel level, Action<TraceRecord> traceAction, [CallerMemberName] string member = "", [CallerLineNumber] int line = 0)
        {
            // Check if this log level is enabled 
            if (level == LogLevel.Trace && log.IsTraceEnabled == false ||
                level == LogLevel.Debug && log.IsDebugEnabled == false ||
               level == LogLevel.Info && (log.IsInfoEnabled == false) ||
               level == LogLevel.Warn && (log.IsWarnEnabled == false) ||
               level == LogLevel.Error && (log.IsErrorEnabled == false) ||
               level == LogLevel.Fatal && (log.IsFatalEnabled == false))
            {
                return;
            }

            TraceRecord tr = new TraceRecord() { Level = level };
            traceAction(tr);
            string message = String.Format("{0}() line {1}: {2}.{3}", member, line, tr.Message, (tr.Data != null) ? Newtonsoft.Json.JsonConvert.SerializeObject(tr.Data) : "");
            
            switch (level)
            {
                case LogLevel.Trace: log.Trace(message, tr.Exception); break;
                case LogLevel.Debug: log.Debug(message, tr.Exception); break;
                case LogLevel.Error: log.Error(message, tr.Exception); break;
                case LogLevel.Fatal: log.Fatal(message, tr.Exception); break;
                case LogLevel.Info: log.Info(message, tr.Exception); break;
                case LogLevel.Warn: log.Warn(message, tr.Exception); break;
            }
        }
    }
}