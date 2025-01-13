using OrderAPI.Models;

namespace OrderAPI.Dtos
{
    public class OrderDto
    {
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    }
}