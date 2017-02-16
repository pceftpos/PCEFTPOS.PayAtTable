using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Configuration;
using Common.Logging;
using System.Web.Http.Tracing;


namespace PayAtTable.API.Helpers
{
    /// <summary>
    /// ITraceWriter implementation for Log4Net
    /// </summary>
    public class Log4NetTracer : ITraceWriter
    {
        private static readonly ILog log = LogManager.GetLogger("SystemDiagnostics");

        public void Trace(HttpRequestMessage request, string category, System.Web.Http.Tracing.TraceLevel level, Action<System.Web.Http.Tracing.TraceRecord> traceAction)
        {
            // Check if this log level is enabled in our Log4Net settings
            if (level == TraceLevel.Debug && log.IsDebugEnabled == false ||
               level == TraceLevel.Info && (log.IsInfoEnabled == false) ||
               level == TraceLevel.Warn && (log.IsWarnEnabled == false) ||
               level == TraceLevel.Error && (log.IsErrorEnabled == false) ||
               level == TraceLevel.Fatal && (log.IsFatalEnabled == false))
            {
                return;
            }

            // Create trace record and populate details
            var rec = new System.Web.Http.Tracing.TraceRecord(request, category, level);
            traceAction(rec);

            // Change unwanted "Info" traces to Debug
            if (rec.Level == TraceLevel.Info)
            {
                // Operation == "" on the first and last trace message
                if (rec.Operation == null || rec.Operation.Length > 0)
                {
                    rec.Level = TraceLevel.Debug;
                    // Return if we have changed this to debug and Log4Net doesn't have debug enabled
                    if (log.IsDebugEnabled == false)
                    {
                        return;
                    }
                }
            }

            var msg = String.Format("{0} {1} {2}", rec.RequestId, rec.Kind.ToString(), rec.Message);
            switch (rec.Level)
            {
                case TraceLevel.Debug:
                    log.Debug(msg);
                    break;
                case TraceLevel.Info:
                    log.Info(msg);
                    break;
                case TraceLevel.Warn:
                    log.Warn(msg);
                    break;
                case TraceLevel.Error:
                    log.Error(msg, rec.Exception);
                    break;
                case TraceLevel.Fatal:
                    log.Fatal(msg, rec.Exception);
                    break;
            }
        }
    }
}