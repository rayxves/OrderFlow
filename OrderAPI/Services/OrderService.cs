using Microsoft.EntityFrameworkCore;
using OrderAPI.Data;
using OrderAPI.Dtos;
using OrderAPI.Interfaces;
using OrderAPI.Models;

namespace OrderAPI.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDBContext _context;
        public OrderService(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<Order> CreateOrderAsync(User user, OrderDto orderDto)
        {
            var order = new Order
            {
                User = user,
                UserId = user.Id,
                OrderDate = DateTime.Now,
                Status = "Pending",
                Total = orderDto.OrderItems.Sum(item => item.Price * item.Quantity),
                OrderItems = orderDto.OrderItems
            };

            await _context.AddAsync(order);
            await _context.SaveChangesAsync();
            return order;
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
            var orders = await _context.Orders.Where(o => o.UserId == userId).ToListAsync();
            if (orders.Count == 0)
            {
                throw new InvalidOperationException("No orders found for this user!");
            }
            return orders;
        }

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                throw new InvalidOperationException("Order not found!");
            }
            return order;
        }

        public async Task<Order> UpdateOrderAsync(int orderId, OrderDto orderDto)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                throw new InvalidOperationException("Order not found!");
            }
            order.OrderItems = orderDto.OrderItems;
            order.Total = orderDto.OrderItems.Sum(item => item.Price * item.Quantity);
            order.OrderDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return order;
        }
    }
}