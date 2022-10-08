namespace Processor.Api.AppSettings
{
    public class StorageSettings
    {

        public string BucketName { get; set; }
        public string BucketFolder { get; set; }
        public long MaxFileSize { get; set; }
    }
}
