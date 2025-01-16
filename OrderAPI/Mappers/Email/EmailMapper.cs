namespace OrderAPI.Mappers
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string EmailFrom { get; set; }
        public string EmailPassword { get; set; }
    }

}