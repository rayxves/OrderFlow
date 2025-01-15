using OrderAPI.Models;

namespace OrderAPI.Interfaces
{
    public interface IDeliveryService
    {
        Task<Delivery> CreateDeliveryAsync(int orderId, int addressId);
        Task<List<Delivery>> GetDeliveriesByUserAsync(string userId);
    }
}