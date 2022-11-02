
using App.Metrics.AspNetCore;
using App.Metrics.Formatters.Prometheus;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog;
using Processor.Api;
using Processor.Api.Middlewares;
using Amazon.S3;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http.Features;
using Processor.Api.Services;

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

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = Int32.MaxValue; // if don't set default value is: 30 MB
});
builder.Services.Configure<FormOptions>(options =>
{
    options.MemoryBufferThreshold = Int32.MaxValue;
    options.MultipartBoundaryLengthLimit = Int32.MaxValue;
    options.MultipartBodyLengthLimit = Int32.MaxValue;
    options.MultipartHeadersLengthLimit = Int32.MaxValue;
});
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddControllers().AddDapr();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IStorageService, StorageService>();

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddTransient<TransferUtility>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseAuthorization();
//app.UseJwtParser();

app.UseCloudEvents();
app.MapControllers();
app.MapSubscribeHandler();



app.Run();
