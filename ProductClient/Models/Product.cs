using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ProductClient.Models;

public class Product
{
  [JsonPropertyName("id")]
  public int Id { get; set; }

  [JsonPropertyName("title")]
  public string Title { get; set; } = string.Empty;

  [JsonPropertyName("price")]
  public decimal Price { get; set; }

  [JsonPropertyName("description")]
  public string Description { get; set; } = string.Empty;

  [JsonPropertyName("category")]
  public string Category { get; set; } = string.Empty;

  [JsonPropertyName("image")]
  public string Image { get; set; } = string.Empty;

  [JsonPropertyName("rating_id")]
  public int RatingId { get; set; }

  [JsonPropertyName("rating")]
  public Rating Rating { get; set; }

  [JsonPropertyName("inventories")]
  public ICollection<Inventory> Inventories { get; set; }
}




