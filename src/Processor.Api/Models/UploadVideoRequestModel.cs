namespace Processor.Api.Models
{
    public class UploadVideoRequestModel
    {
        public DateTime ActualCreatedDate { get; set; }

        public IFormFile File { get; set; }
        public string Name { get; set; }

    }
}
