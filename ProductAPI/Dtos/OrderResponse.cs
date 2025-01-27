namespace ProductAPI.Dtos
{
    public class OrderResponse
    {
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public decimal Total { get; set; }
        public DateTime OrderDate { get; set; }
        public ICollection<OrderItemResponse> OrderItems { get; set; }
    }
}

