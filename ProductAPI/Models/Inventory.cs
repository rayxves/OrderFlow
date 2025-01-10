namespace ProductAPI.Models
{
    public class Inventory
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public int ProductId { get; set; }  // Chave estrangeira
        public Product Product { get; set; }
    }
}