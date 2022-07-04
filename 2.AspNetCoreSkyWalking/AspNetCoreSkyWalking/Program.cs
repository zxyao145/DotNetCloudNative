using Microsoft.AspNetCore.Mvc;
using SkyApm.Tracing;
using SkyApm.Utilities.DependencyInjection;
using Serilog;
using AspNetCoreSkyWalking;


Environment.SetEnvironmentVariable("ASPNETCORE_HOSTINGSTARTUPASSEMBLIES", "SkyAPM.Agent.AspNetCore");

var builder = WebApplication.CreateBuilder(args);

// ���� Serilog
builder.Host.UseSerilog((ctx, serviceProvider, logConfig) =>
    {
        logConfig
            .Enrich.WithSwEnricher(serviceProvider)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:o} {Level:u3}]\t{TraceId}\t{Message}{NewLine}{Exception}"
            )
            .WriteTo.Skywalking(serviceProvider);
    }
);


// ����ͨ�� builder.Configuration ��ȡ SKYWALKING__SERVICENAME
Environment.SetEnvironmentVariable("SKYWALKING__SERVICENAME", "AspNetCoreSwDemo");

var app = builder.Build();


var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async ([FromServices] IEntrySegmentContextAccessor segContext) =>
{
    // ����������ʹ�� HttpClientFactory
    using HttpClient httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri("https://www.baidu.com/");
    var html = await httpClient.GetStringAsync("/");


    var curTraceId = segContext.Context.TraceId;
    segContext.Context.Span.AddLog(SkyApm.Tracing.Segments.LogEvent.Message($"��ǰTreacId��{curTraceId}"));

    app.Logger.LogError(new Exception("LogError Exception"), "this Error");
    Thread.Sleep(1000);
    try
    {
        var a = 0;
        var b = 1 / a;
    }
    catch (Exception e)
    {
        // ��Ӧ Serilog �� Fatal 
        app.Logger.LogCritical(e, "this Critical");
    }



    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
});

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}