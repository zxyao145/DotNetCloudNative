using Serilog.Configuration;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using SkyApm.Tracing;
using SkyApm.Transport;

namespace AspNetCoreSkyWalking;

public static class SwSinkExt
{
    public static LoggerConfiguration Skywalking(
        this LoggerSinkConfiguration sinkConfig,
        IServiceProvider serviceProvider
        )
    {
        ArgumentNullException.ThrowIfNull(nameof(sinkConfig));
        return sinkConfig.Sink(new SwSink(serviceProvider));
    }
}


public class SwSink : ILogEventSink
{
    //private readonly ISkyApmLogDispatcher _skyApmLogDispatcher;

    private readonly IEntrySegmentContextAccessor _segmentContextAccessor;

    public SwSink(IServiceProvider serviceProvider)
    {
        //_skyApmLogDispatcher = serviceProvider.GetRequiredService<ISkyApmLogDispatcher>();
        _segmentContextAccessor = serviceProvider.GetRequiredService<IEntrySegmentContextAccessor>();
    }

    public void Emit(Serilog.Events.LogEvent logEvent)
    {
        var logLevel = logEvent.Level;
        if (logLevel == LogEventLevel.Error
            || logLevel == LogEventLevel.Fatal)
        {
            //var properties = logEvent.Properties;
            //var traceId = properties["TraceId"];
            //var requestPath = properties["RequestPath"];
            var logMsg = logEvent.RenderMessage();

            var swLogMsg = SkyApm.Tracing.Segments.LogEvent.Message(logMsg);
            var swLogKind = SkyApm.Tracing.Segments.LogEvent.ErrorKind(logLevel.ToString("G"));
            var swLogStatck = SkyApm.Tracing.Segments.LogEvent.ErrorStack(logEvent.Exception.ToString());
            _segmentContextAccessor.Context.Span.AddLog(swLogMsg, swLogKind, swLogStatck);

            // Extracted from SkyApmLogger
            // more see https://github.com/SkyAPM/SkyAPM-dotnet/blob/9da12cbffc/src/SkyApm.Utilities.Logging/SkyApmLogger.cs
            //var logInfo = new Dictionary<string, object>();
            //var logMsg = logEvent.RenderMessage();
            //if (logEvent.Exception != null)
            //{
            //    logMsg += Environment.NewLine + logEvent.Exception.ToString();
            //}

            //logInfo.Add("Level", logLevel.ToString("G"));
            //logInfo.Add("logMessage", logMsg);

            //var logContext = new SkyApm.Tracing.Segments.LoggerContext()
            //{
            //    Logs = logInfo,
            //    SegmentContext = _segmentContextAccessor.Context,
            //    Date = DateTimeOffset.UtcNow.Offset.Ticks
            //};
            //_skyApmLogDispatcher.Dispatch(logContext);
        }
    }
}
