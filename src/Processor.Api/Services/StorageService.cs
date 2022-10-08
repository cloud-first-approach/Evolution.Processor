using Amazon.S3;
using Amazon.S3.Transfer;
using System.Net;
using Processor.Api.Models;

namespace Processor.Api.Services
{
    public class StorageService : IStorageService
    {
        private readonly TransferUtility _transferUtility;
        private readonly IConfiguration _config;
        private readonly ILogger<StorageService> _logger;

        IAmazonS3 S3Client { get; set; }
        public StorageService(IAmazonS3 s3Client, TransferUtility transferUtility, IConfiguration configuration, ILogger<StorageService> logger)
        {
            S3Client = s3Client;
            _transferUtility = transferUtility;
            _config = configuration;
            _logger = logger;
        }
        public async Task SaveVideo(SaveProcessedVideoRequestModel videoRequestModel)
        {

            throw new NotImplementedException();    

        }

        public async Task GetVideo(GetSavedVideo videoRequestModel)
        {

            throw new NotImplementedException();

        }

        public async Task<string> UploadVideoToS3BucketAsync(UploadVideoRequestModel requestDto)
        {
            try
            {
                var file = requestDto.File;
                string bucketName = "evolution-video-uploads";

                            // Rename file to random string to prevent injection and similar security threats
                var trustedFileName = WebUtility.HtmlEncode(file.FileName);
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                var randomFileName = Path.GetRandomFileName();
                var trustedStorageName = "files/" + randomFileName + ext;

                // Create the image object to be uploaded in memory
                var transferUtilityRequest = new TransferUtilityUploadRequest()
                {
                    InputStream = file.OpenReadStream(),
                    Key = trustedStorageName,
                    BucketName = bucketName,
                };


                transferUtilityRequest.Metadata.Add("originalFileName", trustedFileName);

               // S3Client.UploadObjectFromStreamAsync(
                await _transferUtility.UploadAsync(transferUtilityRequest);

                // Retrieve Url
                var ImageUrl = GenerateAwsFileUrl(bucketName, trustedStorageName);

                _logger.LogInformation("File uploaded to Amazon S3 bucket successfully");

                return ImageUrl;
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

        private string GenerateAwsFileUrl(string bucketName, string key, bool useRegion = true)
        {
            // URL patterns: Virtual hosted style and path style
            // Virtual hosted style
            // 1. http://[bucketName].[regionName].amazonaws.com/[key]
            // 2. https://[bucketName].s3.amazonaws.com/[key]

            // Path style: DEPRECATED
            // 3. http://s3.[regionName].amazonaws.com/[bucketName]/[key]
            string publicUrl = string.Empty;
            if (useRegion)
            {
                publicUrl = $"https://{bucketName}.s3.{_config.GetSection("AWS").GetValue<string>("Region")}.amazonaws.com/{key}";
            }
            else
            {
                publicUrl = $"https://{bucketName}.s3.ap-south-1.amazonaws.com/{key}";
            }
            return publicUrl;
        }
    }
}
