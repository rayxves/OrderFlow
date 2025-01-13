namespace ProductAPI.Dtos
{
    public class ProductDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public string Image { get; set; }
        public RatingDto Rating { get; set; }
        public InventoryDto Inventory { get; set; }
    }

    public class RatingDto
    {
        public decimal Rate { get; set; }
        public int Count { get; set; }
    }

    public class InventoryDto
    {
        public int Quantity { get; set; }
    }
}
