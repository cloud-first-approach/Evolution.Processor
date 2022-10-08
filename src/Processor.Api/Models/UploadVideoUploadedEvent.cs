namespace Processor.Api.Models
{
    public class UploadVideoUploadedEvent
    {
        public string BucketName { get; set; }
        public string? Region { get; set; }
        public string Bucketkey { get; set; }
    }
}
