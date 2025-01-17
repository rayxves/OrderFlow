using OrderAPI.Models;

namespace OrderAPI.Interfaces
{
    public interface IPaymentService
    {
        Task<Payment> CreatePaymentAsync(int orderId);
    }
}