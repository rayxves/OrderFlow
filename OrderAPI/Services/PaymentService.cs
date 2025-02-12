
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OrderAPI.Data;
using OrderAPI.Dtos;
using OrderAPI.Interfaces;
using OrderAPI.Models;

using ProductClient.Interfaces;
using ProductClient.Models;
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

        public async Task<PaymentSuccessDto> GetPaymentData(string sessionId)
        {
            var payment = await _context.Payments
            .Include(p => p.Order)
                .ThenInclude(o => o.User)
            .Include(p => p.Order)
                .ThenInclude(o => o.Delivery)
            .Include(p => p.Order)
                .ThenInclude(o => o.OrderItems)
            .FirstOrDefaultAsync(p => p.StripeSessionId == sessionId);

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

        public async Task onPaymentFailure(User user, Order order, string orderCancelationReason)
        {
            Console.WriteLine("processando erro");
            var subject = _config["EmailSettings:EmailFrom"];
            var toEmail = user.Email;
            var paymentStatus = "Canceled";
            var deliveryStatus = "";

            var deliveryDate = order.Delivery.DeliveryDate;

            if (order.OrderItems == null || !order.OrderItems.Any())
            {
                Console.WriteLine("Nenhum item no pedido.");
                return;
            }


            var body = _emailService.GenerateEmailHtmlToPayment(user.UserName, order.Id, order.OrderDate, paymentStatus, order.Total, deliveryStatus, deliveryDate, orderCancelationReason);

            await _emailService.SendEmailAsync(toEmail, subject, body);

            order.Status = "Payment Canceled";
            var delivery = order.Delivery;
            delivery.Status = "Canceled";
            var payment = order.Payment;
            payment.PaymentStatus = "Canceled";
            await _context.SaveChangesAsync();
        }

        public async Task onPaymentSuccess(User user, Order order)
        {
            var orderMessageDto = new OrderMessageDto
            {
                OrderId = order.Id,
                UserId = order.UserId,
                Total = order.Total,
                OrderDate = order.OrderDate,
                OrderItems = new List<OrderItemMessageDto>()
            };

            foreach (var orderItem in order.OrderItems)
            {
                var product = await _productService.GetProductsByNameAsync(orderItem.ProductName);

                if (product == null)
                {
                    Console.WriteLine($"Produto não encontrado: {orderItem.ProductName}");
                    continue;
                }


                var orderItemMessageDto = new OrderItemMessageDto
                {
                    ProductId = product.Id,
                    Quantity = orderItem.Quantity
                };


                orderMessageDto.OrderItems.Add(orderItemMessageDto);
            }


            if (orderMessageDto.OrderItems.Any())
            {
                try
                {

                    var inventoryUpdated = await _rabbitMqService.SendMessageAsync("inventory_update", orderMessageDto);
                    Console.WriteLine($"Inventory updated: {inventoryUpdated}");
                    if (!inventoryUpdated)
                    {

                        var failureError = "Estoque vazio/esgotado.";
                        await onPaymentFailure(user, order, failureError);
                        throw new InvalidOperationException(failureError);
                    }
                    else
                    {

                        order.Status = "Payment Success";
                        var delivery = order.Delivery;
                        delivery.Status = "Shipped";
                        var payment = order.Payment;
                        payment.PaymentStatus = "Approved";
                        await _context.SaveChangesAsync();

                        var subject = _config["EmailSettings:EmailFrom"];
                        var toEmail = user.Email;
                        var paymentStatus = "Approved";
                        var deliveryStatus = "Shipped";

                        var deliveryDate = order.Delivery.DeliveryDate;

                        if (order.OrderItems == null || !order.OrderItems.Any())
                        {
                            Console.WriteLine("Nenhum item no pedido.");
                            return;
                        }

                        var body = _emailService.GenerateEmailHtmlToPayment(user.UserName, order.Id, order.OrderDate, paymentStatus, order.Total, deliveryStatus, deliveryDate, "");

                        await _emailService.SendEmailAsync(toEmail, subject, body);


                    }
                }
                catch (System.Exception ex)
                {
                    await onPaymentFailure(user, order, "Erro durante a atualização do estoque.");
                    throw new ArgumentException("Error trying to update inventory: " + ex.Message);

                }
            }



        }

    }
}