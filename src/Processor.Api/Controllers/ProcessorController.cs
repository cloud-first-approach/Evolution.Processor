using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Processor.Api.Models;
using Processor.Api.Services;
using Processor.Api.Services.Models;
using System.Text.Json.Serialization;

namespace Processor.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProcessorController : ControllerBase
    {
        private readonly ILogger<ProcessorController> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IStorageService _storageService;
        private readonly DaprClient _daprClient;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly IConfiguration _configuration;

        public ProcessorController(
            ILogger<ProcessorController> logger,
            IServiceScopeFactory serviceScopeFactory,
            IStorageService storageService,
            DaprClient daprClient,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration
        )
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _storageService = storageService;
            _daprClient = daprClient;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        [Topic("pubsub", "upload")]
        [HttpPost("process")]
        public async Task<IActionResult> Process(UploadVideoUploadedEvent @event)
        {
            //var collection = _serviceScopeFactory.CreateScope();
            //collection.ServiceProvider.GetRequiredService<ILogger<ProcessorController>>();
            //var storage = collection.ServiceProvider.GetRequiredService<IStorageService>();
            //var details = await storage.GetVideoDetails(new Services.Models.GetVideoDetailsRequestModel()
            //{
            //    Key = @event.Bucketkey
            //});
            _logger.Log(LogLevel.Information, @event.Bucketkey);
            //_logger.Log(LogLevel.Information, details.LastModified.ToString());
            return Ok("event raised");
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] string key, [FromQuery] string bucketName)
        {
            try
            {
                var uploader = _configuration.GetSection("Services").GetValue<string>("Uploader");
                _logger.LogInformation(uploader);
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"{uploader}/uploads/test");
                _logger.LogInformation(response.IsSuccessStatusCode.ToString());
                if (response.IsSuccessStatusCode)
                {
                    return Ok(await response.Content.ReadAsStringAsync());
                }
                else
                {
                    return Ok("error");
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogInformation("ERROR " + ex.Message);
                _logger.LogInformation(ex.StackTrace);
                _logger.LogInformation(ex.InnerException?.StackTrace);
            }

            try
            {
                var client = DaprClient.CreateInvokeHttpClient(appId: "uploader");
                var response = await client.GetFromJsonAsync<GetAllVideosResponseModel>("uploads");

                _logger.LogInformation(Convert.ToString(response?.Videos?.Count));

                CancellationTokenSource source = new CancellationTokenSource();
                _logger.LogInformation("been here");
                var videos = await _daprClient.InvokeMethodAsync<GetAllVideosResponseModel>(
                    HttpMethod.Get,
                    "uploader",
                    "/uploads",
                    source.Token
                );
                _logger.LogInformation("been here too ");
                return Ok(response);
            }
            catch (System.Exception ex)
            {
                _logger.LogInformation("ERROR " + ex.Message);
                _logger.LogInformation(ex.StackTrace);
                _logger.LogInformation(ex.InnerException?.StackTrace);
            }

            // var details = await _storageService.GetVideoDetails(
            //     new Services.Models.GetVideoDetailsRequestModel()
            //     {
            //         Key = key,
            //         BucketName = bucketName
            //     }
            // );
            // _logger.Log(LogLevel.Information, details.Size.ToString());
            // _logger.Log(LogLevel.Information, details.LastModified.ToString());
            return Ok("executed");
        }


        [HttpGet]
        [Route("delay/{delay}")]
        public async Task<IActionResult> GetDelay(int delay)
        {
            try
            {
                var uploader = _configuration.GetSection("Services").GetValue<string>("Uploader");
                _logger.LogInformation(uploader);
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"{uploader}/uploads/delay/{delay}");
                _logger.LogInformation(response.IsSuccessStatusCode.ToString());
                return Ok(await response.Content.ReadAsStringAsync());
            }
            catch (System.Exception ex)
            {
                _logger.LogInformation("ERROR " + ex.Message);
                _logger.LogInformation(ex.StackTrace);
                _logger.LogInformation(ex.InnerException?.StackTrace);
                return NotFound();
            }
        }

        [HttpGet]
        [Route("status/{status}")]
        public async Task<IActionResult> GetStatus(int status)
        {
            try
            {
                var uploader = _configuration.GetSection("Services").GetValue<string>("Uploader");
                _logger.LogInformation(uploader);
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"{uploader}/uploads/status/{status}");
                _logger.LogInformation(response.IsSuccessStatusCode.ToString());
                return Ok(await response.Content.ReadAsStringAsync());
            }
            catch (System.Exception ex)
            {
                _logger.LogInformation("ERROR " + ex.Message);
                _logger.LogInformation(ex.StackTrace);
                _logger.LogInformation(ex.InnerException?.StackTrace);
                return NotFound();
            }
        }
    }
}
