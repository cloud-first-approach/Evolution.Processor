using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Processor.Api.Models;
using Processor.Api.Services;


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

        public ProcessorController(
            ILogger<ProcessorController> logger,
            IServiceScopeFactory serviceScopeFactory,
            IStorageService storageService,
            DaprClient daprClient
        )
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _storageService = storageService;
            _daprClient = daprClient;
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
               
                CancellationTokenSource source = new CancellationTokenSource();

                var videos = await _daprClient.InvokeMethodAsync<UploadApiVideoModel>(HttpMethod.Get,"uploader", "/uploads", source.Token);

                _logger.LogInformation("Key " + videos.Videos[0].Key);
              
            }
            catch (System.Exception ex)
            {
                _logger.LogInformation("First App Id " + ex.Message);
            }
           

            var details = await _storageService.GetVideoDetails(
                new Services.Models.GetVideoDetailsRequestModel()
                {
                    Key = key,
                    BucketName = bucketName
                }
            );
            _logger.Log(LogLevel.Information, details.Size.ToString());
            _logger.Log(LogLevel.Information, details.LastModified.ToString());
            return Ok(details);
        }
    }
}
