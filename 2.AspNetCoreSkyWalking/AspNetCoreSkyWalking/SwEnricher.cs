using Microsoft.AspNetCore.Http;
using Serilog.Configuration;
using Serilog;
using Serilog.Context;
using Serilog.Core;
using Serilog.Events;
using SkyApm.Tracing;

namespace AspNetCoreSkyWalking;

public static class SwEnricherExt
{
    public static LoggerConfiguration WithSwEnricher(
        this LoggerEnrichmentConfiguration enrich,
        IServiceProvider serviceProvider
        )
    {
        ArgumentNullException.ThrowIfNull(nameof(enrich));
        return enrich.With(new SwEnricher(serviceProvider));
    }
}


public class SwEnricher : ILogEventEnricher
{
    private readonly IEntrySegmentContextAccessor _segContext;

    public SwEnricher(IServiceProvider serviceProvider)
    {
        _segContext = serviceProvider.GetRequiredService<IEntrySegmentContextAccessor>();
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var tid = _segContext?.Context?.TraceId ?? "N/A";
        logEvent.AddPropertyIfAbsent(
            propertyFactory.CreateProperty("TraceId", tid)
        );
    }
}
