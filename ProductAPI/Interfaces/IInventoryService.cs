using ProductAPI.Dtos;
using ProductAPI.Models;

namespace ProductAPI.Interfaces
{
    public interface IInventoryService
    {
        Task<bool> UpdateInventoryWithTransactionAsync(IEnumerable<OrderItemResponse> items);
        Task<bool> AddToInventoryAsync(IEnumerable<OrderItemResponse> items);
    }
}