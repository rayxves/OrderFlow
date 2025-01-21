using OrderAPI.Models;

namespace OrderAPI.Dtos
{
    public class PaymentSuccessDto
    {
        public User User { get; set; }
        public Order Order { get; set; }
    }
}