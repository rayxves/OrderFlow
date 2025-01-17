namespace OrderAPI.Dtos
{
    public class OrderResponseDto
{
    public int OrderId { get; set; }
    public string PaymentLink { get; set; }
    public decimal Total { get; set; }
}

}
