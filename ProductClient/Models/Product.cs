using System;

namespace ProductClient.Models;

public class Product
{
  public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public int RatingId { get; set; }
        public Rating Rating { get; set; }
        public ICollection<Inventory> Inventories { get; set; }
    
}
