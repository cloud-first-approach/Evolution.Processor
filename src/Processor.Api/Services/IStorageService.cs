using Processor.Api.Models;
using Processor.Api.Services.Models;

namespace Processor.Api.Services
{
    public interface IStorageService
    {
        Task<GetAllVideosResponseModel> GetAllVideos(GetAllVideosRequestModel videoRequestModel);

        Task<GetVideoDetailsResponseModel> GetVideoDetails(GetVideoDetailsRequestModel videoRequestModel);
        Task SaveVideo(UploadVideoRequestModel videoRequestModel);
        public Task<UploadVideoResponseModel> UploadVideoToS3BucketAsync(UploadVideoRequestModel requestDto);
    }
}