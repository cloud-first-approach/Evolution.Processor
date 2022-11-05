
using App.Metrics.AspNetCore;
using App.Metrics.Formatters.Prometheus;
using Processor.Api.Extensions;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

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

builder.Host.UseMetricsWebTracking()
                .UseMetrics(
                    option =>
                    {
                        option.EndpointOptions = endpointOption =>
                        {
                            endpointOption.MetricsTextEndpointOutputFormatter =
                                new MetricsPrometheusTextOutputFormatter();
                            endpointOption.MetricsEndpointOutputFormatter =
                                new MetricsPrometheusProtobufOutputFormatter();
                            endpointOption.EnvironmentInfoEndpointEnabled = false;
                        };
                    }
                );
builder.BuildServices();
var app = builder.Build();
app.BuildApp();

app.Run();
