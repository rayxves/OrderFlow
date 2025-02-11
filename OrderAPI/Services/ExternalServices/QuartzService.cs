using OrderAPI.Data;
using OrderAPI.Interfaces;
using Quartz;

namespace OrderAPI.Services
{
    public class QuartzService : IJob
    {
        private readonly IDeliveryService _deliveryService;
        private readonly ApplicationDBContext _context;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;
        private readonly ILogger<QuartzService> _logger;

        public QuartzService(
            IDeliveryService deliveryService,
            ApplicationDBContext context,
            EmailService emailService,
            IConfiguration config,
            ILogger<QuartzService> logger)
        {
            _deliveryService = deliveryService;
            _context = context;
            _emailService = emailService;
            _config = config;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var today = DateTime.UtcNow.Date;

            var deliveries = await _deliveryService.GetDeliveriesByDateAndOrderSuccess(today);

            var emailTasks = new List<Task>();

            foreach (var delivery in deliveries)
            {
                if (delivery.Status != "Delivered")
                {
                    delivery.Status = "Delivered";
                    _context.Update(delivery);
                    await _context.SaveChangesAsync();


                    var orderId = delivery.OrderId;
                    var user = delivery.Address.User;
                    if (user == null)
                    {
                        _logger.LogError($"User not found for delivery with orderId {orderId}");
                        continue;
                    }


                    var body = _emailService.GenerateEmailHtmlToDelivery(user.UserName, orderId, "Delivered", delivery.DeliveryDate);
                    var subject = _config["EmailSettings:EmailFrom"] ?? "Delivery Notification";
                    var toEmail = user.Email;

                    emailTasks.Add(Task.Run(async () =>
                    {
                        try
                        {
                            await _emailService.SendEmailAsync(toEmail, subject, body);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"Error sending delivery email to {toEmail}: {ex.Message}");
                        }
                    }));
                }
            }

            await Task.WhenAll(emailTasks);
            await _context.SaveChangesAsync();
        }
    }
}
