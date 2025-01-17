
using Microsoft.EntityFrameworkCore;
using OrderAPI.Data;
using OrderAPI.Dtos;
using OrderAPI.Interfaces;
using OrderAPI.Models;
using Stripe.Checkout;

namespace OrderAPI.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IConfiguration _config;
        private readonly ApplicationDBContext _context;
        private readonly StripeService _stripe;
        private readonly EmailService _emailService;
        private readonly RabbitMqService _rabbitMqService;
        public PaymentService(ApplicationDBContext context, StripeService stripe, EmailService emailService, RabbitMqService rabbitMq, IConfiguration config)
        {
            _context = context;
            _stripe = stripe;
            _emailService = emailService;
            _rabbitMqService = rabbitMq;
            _config = config;
        }
        public async Task<Payment> CreatePaymentAsync(int orderId)
        {
            var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                throw new InvalidOperationException("Order not found!");
            }

            var lineItems = order.OrderItems.Select(item => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "brl",
                    UnitAmount = (long)(item.Price * 100),
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.ProductName,
                    },
                },
                Quantity = item.Quantity,
            }).ToList();

            var payment = new Payment
            {
                OrderId = orderId,
                PaymentDate = DateTime.UtcNow,
                PaymentStatus = "Pending",
                StripePaymentIntentId = await _stripe.CreateCheckoutSessionAsync(lineItems)
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task onPaymentSuccess(User user, Order order, int productId)
        {

            var subject = _config["EmailSettings:EmailFrom"];
            var toEmail = user.Email;
            var status = "Approved";
            var deliveryDate = order.Delivery.DeliveryDate;


            var body = _emailService.GenerateEmailHtml(user.UserName, order.Id, order.OrderDate, status, order.Total, status, deliveryDate);
            var emailService = _emailService.SendEmailAsync(toEmail, subject, body);

            var orderMessageDto = new OrderMessageDto
            {
                OrderId = order.Id,
                UserId = order.UserId,
                Total = order.Total,
                OrderDate = order.OrderDate,
                OrderItems = order.OrderItems.Select(oi => new OrderItemMessageDto
                {
                    ProductId = productId,
                    Quantity = oi.Quantity
                }).ToList()
            };

            await _rabbitMqService.SendMessageAsync("inventory_updates", orderMessageDto);
        }

    }
}