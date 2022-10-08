using Dapr;
using Microsoft.AspNetCore.Mvc;
using Processor.Api.Models;

namespace Processor.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProcessorController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<ProcessorController> _logger;

        public ProcessorController(ILogger<ProcessorController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<string> Get()
        {
            return Summaries;
        }


        [Topic("pubsub", "upload")]
        [HttpPost("process")]
        public async Task<IActionResult> Process(UploadVideoUploadedEvent item)
        {
            _logger.Log(LogLevel.Information,"event raised");
            return Ok("event raised");
        }
    }
}