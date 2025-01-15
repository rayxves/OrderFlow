using OrderAPI.Dtos;
using OrderAPI.Models;

namespace OrderAPI.Interfaces
{
    public interface IOrderItemService
    {
        Task<OrderItem> CreateOrderItemAsync(OrderItemDto orderItemDto, Order order);
    }
}