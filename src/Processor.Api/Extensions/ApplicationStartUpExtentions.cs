using Amazon.S3.Transfer;
using Amazon.S3;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Processor.Api.Services;

namespace Processor.Api.Extensions
{
    public static class ApplicationStartUpExtentions
    {

        public static void BuildServices(this WebApplicationBuilder builder)
        {
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

            builder.Services.AddHealthChecks();
        }

        public static void BuildApp(this WebApplication app)
        {

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            //app.UseAuthorization();
            //app.UseJwtParser();
            app.UseOpenTelemetryPrometheusScrapingEndpoint();

            app.UseCloudEvents();
            app.MapControllers();
            app.MapSubscribeHandler();
            app.UseHealthChecks("/health");
            app.UseHealthChecks("/healthz");

        }
    }
}
