using OrderAPI.Dtos;
using OrderAPI.Models;

namespace OrderAPI.Interfaces
{
    public interface IOrderService{
        Task<OrderResponseDto> CreateOrderAsync(User user, OrderDto orderDto);
        Task<Order> GetOrderByIdAsync(int orderId);
        Task<List<Order>> GetAllOrdersByUserAsync(string userId);
        Task<OrderResponseDto> UpdateOrderAsync(int orderId, OrderDto orderDto);
        Task<Order> DeleteOrderAsync(int orderId);
        
    }
}