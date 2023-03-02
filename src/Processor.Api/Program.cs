
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Processor.Api.Extensions;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Processor.Api;

Log.Logger = new LoggerConfiguration().MinimumLevel
               .Debug()
               .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
               .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
               .MinimumLevel.Override("System", LogEventLevel.Warning)
               .MinimumLevel.Override(
                   "Microsoft.AspNetCore.Authentication",
                   LogEventLevel.Information
               )
               .Enrich.FromLogContext()
               .WriteTo.Console(
                   outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                   theme: AnsiConsoleTheme.Code
               )
               .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Add configuration to the application
builder.Configuration.AddJsonFile("appsettings.k8s.json", optional: true);

// Note: Switch between Zipkin/Jaeger/OTLP/Console by setting UseTracingExporter in appsettings.json.
var tracingExporter = builder.Configuration.GetValue<string>("UseTracingExporter").ToLowerInvariant();

// Note: Switch between Prometheus/OTLP/Console by setting UseMetricsExporter in appsettings.json.
var metricsExporter = builder.Configuration.GetValue<string>("UseMetricsExporter").ToLowerInvariant();

// Note: Switch between Console/OTLP by setting UseLogExporter in appsettings.json.
var logExporter = builder.Configuration.GetValue<string>("UseLogExporter").ToLowerInvariant();

// Build a resource configuration action to set service information.
Action<ResourceBuilder> configureResource = r => r.AddService(
    serviceName: builder.Configuration.GetValue<string>("ServiceName"),
    serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown",
    serviceInstanceId: Environment.MachineName);

// Create a service to expose ActivitySource, and Metric Instruments
// for manual instrumentation
builder.Services.AddSingleton<Instrumentation>();

// Configure OpenTelemetry tracing & metrics with auto-start using the
// AddOpenTelemetry extension from OpenTelemetry.Extensions.Hosting.
builder.Services.AddOpenTelemetry()
    .ConfigureResource(configureResource)
    .WithTracing(otbuilder =>
    {
        // Tracing

        // Ensure the TracerProvider subscribes to any custom ActivitySources.
        otbuilder
            .AddSource(Instrumentation.ActivitySourceName)
            .SetSampler(new AlwaysOnSampler())
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation();

        // Use IConfiguration binding for AspNetCore instrumentation options.
        builder.Services.Configure<AspNetCoreInstrumentationOptions>(builder.Configuration.GetSection("AspNetCoreInstrumentation"));

        switch (tracingExporter)
        {
            case "jaeger":
                otbuilder.AddJaegerExporter();

                otbuilder.ConfigureServices(services =>
                {
                    // Use IConfiguration binding for Jaeger exporter options.
                    services.Configure<JaegerExporterOptions>(builder.Configuration.GetSection("Jaeger"));

                    // Customize the HttpClient that will be used when JaegerExporter is configured for HTTP transport.
                    services.AddHttpClient("JaegerExporter", configureClient: (client) => client.DefaultRequestHeaders.Add("X-MyCustomHeader", "value"));
                });
                break;

            case "zipkin":
                otbuilder.AddZipkinExporter();

                otbuilder.ConfigureServices(services =>
                {
                    // Use IConfiguration binding for Zipkin exporter options.
                    services.Configure<ZipkinExporterOptions>(builder.Configuration.GetSection("Zipkin"));
                });
                break;

            //case "otlp":
            //    builder.AddOtlpExporter(otlpOptions =>
            //    {
            //        // Use IConfiguration directly for Otlp exporter endpoint option.
            //        otlpOptions.Endpoint = new Uri(appBuilder.Configuration.GetValue<string>("Otlp:Endpoint"));
            //    });
            //    break;

            default:
                otbuilder.AddConsoleExporter();
                break;
        }
    })
    .WithMetrics(builder =>
    {
        // Metrics

        // Ensure the MeterProvider subscribes to any custom Meters.
        builder
            .AddMeter(Instrumentation.MeterName)
            .AddRuntimeInstrumentation()
            .AddHttpClientInstrumentation()
            .AddAspNetCoreInstrumentation();

        switch (metricsExporter)
        {
            case "prometheus":
                builder.AddPrometheusExporter();
                break;
            //case "otlp":
            //    builder.AddOtlpExporter(otlpOptions =>
            //    {
            //        // Use IConfiguration directly for Otlp exporter endpoint option.
            //        otlpOptions.Endpoint = new Uri(appBuilder.Configuration.GetValue<string>("Otlp:Endpoint"));
            //    });
            //    break;
            default:
                builder.AddConsoleExporter();
                break;
        }
    });

builder.BuildServices();
var app = builder.Build();

app.BuildApp();

app.Run();
