namespace Processor.Api.Models
{
    public class UploadApiVideoModel
    {
        public List<VideoModel> Videos;
    }

    public class VideoModel
    {
        public DateTime LastModified { get; set; }
        public long Size { get; set; }
        public string Key { get; set; }
          
    }
}
