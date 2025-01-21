
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OrderAPI.Data;
using OrderAPI.Dtos;
using OrderAPI.Interfaces;
using OrderAPI.Models;

using ProductClient.Interfaces;
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
        private readonly IProductService _productService;
        private Task emailServiceTask;

        public PaymentService(ApplicationDBContext context, StripeService stripe, EmailService emailService, RabbitMqService rabbitMq, IConfiguration config, IProductService productService)
        {
            _context = context;
            _stripe = stripe;
            _emailService = emailService;
            _rabbitMqService = rabbitMq;
            _config = config;
            _productService = productService;
        }
        public async Task<Payment> CreatePaymentAsync(int orderId)
        {
            var order = await _context.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId);

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

            var session = await _stripe.CreateCheckoutSessionAsync(lineItems);
            var payment = new Payment
            {
                OrderId = orderId,
                Order = order,
                PaymentDate = DateTime.UtcNow,
                PaymentStatus = "Pending",
                PaymentUrl = session.Url,
                StripeSessionId = session.Id,
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();
            return payment;
        }

        public async Task<PaymentSuccessDto> GetPaymentSuccessData(string sessionId)
        {

            var payment = await _context.Payments
            .Include(p => p.Order)
                .ThenInclude(o => o.User)
            .Include(p => p.Order)
                .ThenInclude(o => o.Delivery)
            .Include(p => p.Order)
                .ThenInclude(o => o.OrderItems)
            .FirstOrDefaultAsync(p => p.StripeSessionId == sessionId);

            Console.WriteLine($"StripeSessionId do banco: {payment.StripeSessionId}");

            if (payment.Order == null)
            {
                throw new ArgumentNullException(nameof(payment.Order));
            }
            else if (payment.Order.User == null)
            {
                throw new ArgumentNullException(nameof(payment.Order.User));
            }

            if (payment == null)
            {
                throw new ArgumentNullException(nameof(payment));
            }


            var order = payment.Order;
            var user = order.User;



            return new PaymentSuccessDto
            {
                User = user,
                Order = order,
            };
        }

        public async Task onPaymentSuccess(User user, Order order)
        {
            Console.WriteLine(user.ToString());
            Console.WriteLine(order.ToString());
            var subject = _config["EmailSettings:EmailFrom"];
            var toEmail = user.Email;
            var paymentStatus = "Approved";
            var deliveryStatus = "Shipped";
            Console.WriteLine(order.Total);
            var deliveryDate = order.Delivery.DeliveryDate;

            if (order.OrderItems == null || !order.OrderItems.Any())
            {
                Console.WriteLine("Nenhum item no pedido.");
                return;
            }


            var body = _emailService.GenerateEmailHtml(user.UserName, order.Id, order.OrderDate, paymentStatus, order.Total, deliveryStatus, deliveryDate);
            Console.WriteLine("chamando email");
            Console.WriteLine(body);
        
    
            await _emailService.SendEmailAsync(toEmail, subject, body);
            Console.WriteLine("deu certo email");


            foreach (var orderItem in order.OrderItems)
            {
                Console.WriteLine(orderItem.ProductName);
                Console.WriteLine(orderItem.Quantity);
                Console.WriteLine(orderItem.Price);

                var product = await _productService.GetProductsByNameAsync(orderItem.ProductName);
                Console.WriteLine(product);

                if (product == null)
                {
                    Console.WriteLine($"Produto não encontrado: {orderItem.ProductName}");
                    continue;
                }

                var orderMessageDto = new OrderMessageDto
                {
                    OrderId = order.Id,
                    UserId = order.UserId,
                    Total = order.Total,
                    OrderDate = order.OrderDate,
                    OrderItems = new List<OrderItemMessageDto>
            {
                new OrderItemMessageDto
                {
                    ProductId = product.Id,
                    Quantity = orderItem.Quantity
                }
            }
                };

                order.Status = "Payment Success";
                var delivery = order.Delivery;
                delivery.Status = "Shipped";
                await _context.SaveChangesAsync();

                Console.WriteLine($"Enviando mensagem para RabbitMQ: {JsonConvert.SerializeObject(orderMessageDto)}");

                await _rabbitMqService.SendMessageAsync("inventory_update", orderMessageDto);
            }
        }

    }
}