namespace OrderAPI.Mappers
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string EmailFrom { get; set; } = string.Empty;
        public string EmailPassword { get; set; } = string.Empty;
    }

}