namespace Processor.Api.Services.Models
{
    public class UploadVideoRequestModel
    {
        public DateTime ActualCreatedDate { get; set; }
        public string? UserId { get; set; }
         public string? Username { get; set; }
        public IFormFile File { get; set; }
        public string Description { get; set; }

        public string? BucketName { get; set; }
        public string? BucketFolder { get; set; }

    }
}