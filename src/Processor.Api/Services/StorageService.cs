using Amazon.S3;
using Amazon.S3.Transfer;
using System.Net;
using Processor.Api.Models;
using Processor.Api.AppSettings;
using Processor.Api.Services.Models;
using Microsoft.Extensions.Options;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using System.Security.AccessControl;

namespace Processor.Api.Services
{
    public class StorageService : IStorageService
    {
        private readonly TransferUtility _transferUtility;
        private readonly IConfiguration _config;
        private readonly ILogger<StorageService> _logger;
        private readonly StorageSettings _storageSettings;

        IAmazonS3 S3Client { get; set; }
        public StorageService(IAmazonS3 s3Client, TransferUtility transferUtility, IConfiguration configuration, ILogger<StorageService> logger, IOptions<StorageSettings> storageSettings)
        {
            S3Client = s3Client;
            _transferUtility = transferUtility;
            _config = configuration;
            _logger = logger;
            _storageSettings = storageSettings.Value;
        }
        public async Task SaveVideo(UploadVideoRequestModel videoRequestModel)
        {

            var filePath = Path.GetTempFileName();

            using (var stream = System.IO.File.Create(filePath))
            {
                await videoRequestModel.File.CopyToAsync(stream);
            }

        }

        public async Task<UploadVideoResponseModel> UploadVideoToS3BucketAsync(UploadVideoRequestModel requestDto)
        {
            try
            {
                var file = requestDto.File;
                string bucketName = _storageSettings.BucketName;

                // Rename file to random string to prevent injection and similar security threats
                var trustedFileName = WebUtility.HtmlEncode(file.FileName);
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                var randomFileName = Path.GetRandomFileName();
                var trustedStorageKey = $"{_storageSettings.BucketFolder}/{randomFileName}{ext}";

                // Create the image object to be uploaded in memory
                var transferUtilityRequest = new TransferUtilityUploadRequest()
                {
                    InputStream = file.OpenReadStream(),
                    Key = trustedStorageKey,
                    BucketName = bucketName,
                };

                transferUtilityRequest.Metadata.Add("originalFileName", trustedFileName);

                // S3Client.UploadObjectFromStreamAsync(
                await _transferUtility.UploadAsync(transferUtilityRequest);

                _logger.LogInformation("File uploaded to Amazon S3 bucket successfully");

                return new UploadVideoResponseModel() { 
                        Bucketkey = trustedStorageKey, 
                        BucketName=bucketName,
                };
            }
            catch (Exception ex) when (ex is NullReferenceException)
            {
                _logger.LogError("File data not contained in form", ex);
                throw;
            }
            catch (Exception ex) when (ex is AmazonS3Exception)
            {
                _logger.LogError("Something went wrong during file upload", ex);
                throw;
            }

        }

        public async Task<GetAllVideosResponseModel> GetAllVideos(GetAllVideosRequestModel videoRequestModel)
        {

            var response = await S3Client.ListObjectsV2Async(new ListObjectsV2Request() { 
                BucketName = _storageSettings.BucketName,
                MaxKeys = 10,
                Prefix = _storageSettings.BucketFolder
            });

            var Videos = new List<VideoData>();
            foreach (var obj in response.S3Objects)
            {
                var data = new VideoData()
                {
                    Key = obj.Key,
                    Size = obj.Size,
                    LastModified = obj.LastModified
                };
                Videos.Add(data);
            }

            return new GetAllVideosResponseModel()
            {
                Videos = Videos
            }; 

        }

        public async Task<GetVideoDetailsResponseModel> GetVideoDetails(GetVideoDetailsRequestModel videoRequestModel)
        {
            var response = await S3Client.GetObjectAsync(videoRequestModel.BucketName,videoRequestModel.Key);

            // await response.WriteResponseStreamToFileAsync($"{filePath}\\{objectName}", true, System.Threading.CancellationToken.None);

            var details = new GetVideoDetailsResponseModel()
            {
                Key = response.Key,
                LastModified = response.LastModified,
                Size = response.ContentLength
            };
            

            return details;
        }
    }
}
