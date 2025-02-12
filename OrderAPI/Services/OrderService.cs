using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Data;
using OrderAPI.Dtos;
using OrderAPI.Interfaces;
using OrderAPI.Mappers;
using OrderAPI.Models;

namespace OrderAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDBContext _context;
        private readonly IOrderItemService _orderItemService;
        private readonly IDeliveryService _deliveryService;
        private readonly IPaymentService _paymentService;
        private readonly EmailService _emailService;
        private readonly IConfiguration _config;
        public OrderService(ApplicationDBContext context, IOrderItemService orderItemService, IDeliveryService deliveryService, IPaymentService paymentService, EmailService emailService, IConfiguration config)
        {
            _context = context;
            _orderItemService = orderItemService;
            _deliveryService = deliveryService;
            _paymentService = paymentService;
            _emailService = emailService;
            _config = config;
        }
        public async Task<OrderResponseDto> CreateOrderAsync(User user, OrderDto orderDto)
        {

            var order = new Order
            {
                User = user,
                UserId = user.Id,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                Total = 0

            };

            var address = await _context.Addresses.Where(a => a.Id == orderDto.AddressId).FirstOrDefaultAsync();
            if (address == null)
            {
                throw new ArgumentNullException("Endereço não encontrado.");
            }

            foreach (var item in orderDto.OrderItems)
            {
                var orderItem = await _orderItemService.CreateOrderItemAsync(item, order);
                order.Total += orderItem.Price * orderItem.Quantity;
                order.OrderItems.Add(orderItem);
            }

            await _context.AddAsync(order);
            await _context.SaveChangesAsync();

            var delivery = await _deliveryService.CreateDeliveryAsync(order.Id, orderDto.AddressId);
            if (delivery == null)
            {
                _context.Remove(order);
                await _context.SaveChangesAsync();
                throw new InvalidOperationException("Error creating delivery!");
            }

            var payment = await _paymentService.CreatePaymentAsync(order.Id);

            if (payment == null)
            {
                _context.Remove(order);
                await _context.SaveChangesAsync();
                throw new InvalidOperationException("Error creating payment!");
            }

            order.Delivery = delivery;
            order.Payment = payment;

            _context.Update(order);
            await _context.SaveChangesAsync();
            return new OrderResponseDto
            {
                OrderId = order.Id,
                PaymentLink = payment.PaymentUrl,
                Total = order.Total
            };
        }

        public async Task<Order> DeleteOrderAsync(int orderId)
        {
            var order = await _context.Orders.Include(o => o.User).Include(o => o.Delivery).Include(o => o.Payment).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                throw new InvalidOperationException("Order not found!");
            }


            try
            {
                var subject = _config["EmailSettings:EmailFrom"];
                var toEmail = order.User.Email;
                var delivery = order.Delivery;
                var payment = order.Payment;
                var orderCancelationReason = "Recebemos o cancelamento do pedido, o dinheiro será devolvido em até 3 dias úteis.";
                var body = _emailService.GenerateEmailHtmlToPayment(order.User.UserName, order.Id, order.OrderDate, payment.PaymentStatus, order.Total, delivery.Status, delivery.DeliveryDate, orderCancelationReason);
                await _emailService.SendEmailAsync(toEmail, subject, body);
            }
            catch (System.Exception ex)
            {
                throw new InvalidOperationException("Error trying to send email. Order has been cancelled. ");
            }

            _context.Remove(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<List<Order>> GetAllOrdersByUserAsync(string userId)
        {
            var orders = await _context.Orders.Where(o => o.UserId == userId).Include(o => o.OrderItems).ToListAsync();
            if (orders.Count == 0)
            {
                throw new InvalidOperationException("No orders found for this user!");
            }
            return orders;
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                throw new InvalidOperationException("Order not found!");
            }
            return order;
        }

        public async Task<OrderResponseDto> UpdateOrderAsync(int orderId, OrderDto orderDto)
        {
            var order = await _context.Orders.Include(o => o.OrderItems).Include(o => o.Delivery).Include(o => o.Payment).FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new InvalidOperationException("Order not found!");
            }

            var delivery = order.Delivery;
            if (delivery.Status == "Delivered" || delivery.Status == "Shipped")
            {
                throw new InvalidOperationException("Order has already been shipped or delivered, please cancel the order and create a new one.");
            }

            if (order.Status == "Payment Success" || order.Status == "Payment Canceled")
            {
                throw new InvalidOperationException("Order has already been processed, please cancel the order and create a new one.");
            }

            order.OrderDate = DateTime.UtcNow;
            order.Total = 0;
            order.OrderItems = new List<OrderItem>();
            _context.Remove(delivery);
            _context.Remove(order.Payment);

            foreach (var item in orderDto.OrderItems)
            {
                var orderItem = await _orderItemService.CreateOrderItemAsync(item, order);
                order.Total += orderItem.Price * orderItem.Quantity;
                order.OrderItems.Add(orderItem);
            }

            await _context.SaveChangesAsync();

            var newDelivery = await _deliveryService.CreateDeliveryAsync(order.Id, orderDto.AddressId);
            if (delivery == null)
            {
                _context.Remove(order);
                await _context.SaveChangesAsync();
                throw new InvalidOperationException("Error creating delivery!");
            }

            var newPayment = await _paymentService.CreatePaymentAsync(order.Id);

            if (newPayment == null)
            {
                _context.Remove(order);
                await _context.SaveChangesAsync();
                throw new InvalidOperationException("Error creating payment!");
            }

            order.Delivery = newDelivery;
            order.Payment = newPayment;

            _context.Update(order);
            await _context.SaveChangesAsync();
            return new OrderResponseDto
            {
                OrderId = order.Id,
                PaymentLink = newPayment.PaymentUrl,
                Total = order.Total
            };
        }
    }
}