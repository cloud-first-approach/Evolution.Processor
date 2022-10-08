

namespace Processor.Api.Services.Models
{
    public class GetAllVideosResponseModel
    {
        public List<VideoData> Videos { get; set; }
    }

    public class VideoData {
        public DateTime LastModified { get; set; }
        public long Size { get; set; }
        public string Key { get; set; }
    }
}
