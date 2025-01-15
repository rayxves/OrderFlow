using OrderAPI.Models;

namespace OrderAPI.Dtos
{
    public class NewOrderDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal Total { get; set; }
        public ICollection<OrderItemDto> OrderItemsDto { get; set; } = new List<OrderItemDto>();

    }
}