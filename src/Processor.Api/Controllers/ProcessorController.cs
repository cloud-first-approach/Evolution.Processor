using Dapr;
using Microsoft.AspNetCore.Mvc;
using Processor.Api.Models;

namespace Processor.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProcessorController : ControllerBase
    {
        private readonly ILogger<ProcessorController> _logger;

        public ProcessorController(ILogger<ProcessorController> logger)
        {
            _logger = logger;
        }

        [Topic("pubsub", "upload")]
        [HttpPost("process")]
        public async Task<IActionResult> Process(UploadVideoUploadedEvent item)
        {
            _logger.Log(LogLevel.Information, item.Url);
            return Ok("event raised");
        }
    }
}