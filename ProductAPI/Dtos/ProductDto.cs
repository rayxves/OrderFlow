namespace ProductAPI.Dtos
{
    public class ProductDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
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
