namespace ProductAPI.Dtos
{
    public class ProductUpdateMessage
    {
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public DateTime Timestamp { get; set; }
    }
}