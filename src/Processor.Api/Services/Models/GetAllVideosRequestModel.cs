namespace Processor.Api.Services.Models
{
    public class GetAllVideosRequestModel
    {
        public string BucketName { get; internal set; }
        public string BucketFolder { get; internal set; }
    }
}