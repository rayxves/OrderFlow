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
        public OrderService(ApplicationDBContext context, IOrderItemService orderItemService, IDeliveryService deliveryService, IPaymentService paymentService)
        {
            _context = context;
            _orderItemService = orderItemService;
            _deliveryService = deliveryService;
            _paymentService = paymentService;
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
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                throw new InvalidOperationException("Order not found!");
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

        public async Task<Order> UpdateOrderAsync(int orderId, OrderDto orderDto)
        {
            var order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                throw new InvalidOperationException("Order not found!");
            }

            var delivery = order.Delivery;
            if (delivery.Status == "Delivered" || delivery.Status == "Shipped")
            {
                throw new InvalidOperationException("Order has already been shipped or delivered ");
            }

            order.OrderDate = DateTime.Now;
            order.Total = 0;
            order.OrderItems = new List<OrderItem>();

            foreach (var item in orderDto.OrderItems)
            {
                var orderItem = await _orderItemService.CreateOrderItemAsync(item, order);
                order.Total += orderItem.Price * orderItem.Quantity;
                order.OrderItems.Add(orderItem);
            }

            await _context.SaveChangesAsync();
            return order;
        }
    }
}