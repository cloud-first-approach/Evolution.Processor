namespace Processor.Api.AppSettings
{
    public class AuthSettings
    {

        public string Secret { get; set; }
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public int ValidDuration { get; set; }
    }
}
