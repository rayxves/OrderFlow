using OrderAPI.Models;

namespace OrderAPI.Dtos
{
    public class OrderDto
    {
        public int AddressId { get; set;}
        public ICollection<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();

    }
}