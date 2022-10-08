using Processor.Api.Models;

namespace Processor.Api.Services
{
    public interface IStorageService
    {
        public Task SaveVideo(SaveProcessedVideoRequestModel videoRequestModel);
        public Task GetVideo(GetSavedVideo videoRequestModel);
        public Task<string> UploadVideoToS3BucketAsync(UploadVideoRequestModel requestDto);
    }
}