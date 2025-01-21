using OrderAPI.Dtos;
using OrderAPI.Models;

namespace OrderAPI.Interfaces
{
    public interface IPaymentService
    {
        Task<Payment> CreatePaymentAsync(int orderId);
        Task onPaymentSuccess(User user, Order order);
        Task<PaymentSuccessDto> GetPaymentSuccessData(string sessionId);
    }
}