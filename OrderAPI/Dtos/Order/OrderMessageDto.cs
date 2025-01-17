namespace OrderAPI.Dtos
{
    public class OrderMessageDto
{
    public int OrderId { get; set; }
    public string UserId { get; set; }
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
    public ICollection<OrderItemMessageDto> OrderItems { get; set; }
}
}