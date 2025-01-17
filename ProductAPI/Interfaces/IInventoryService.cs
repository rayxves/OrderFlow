using ProductAPI.Models;

namespace ProductAPI.Interfaces
{
    public interface IInventoryService
    {
        Task<Inventory> UpdateInventoryAsync(int productId, int quantity);
    }
}