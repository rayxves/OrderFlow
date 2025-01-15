using System;
using System.Text.Json.Serialization;

namespace ProductClient.Models;

public class Inventory
{
  [JsonPropertyName("id")]
  public int Id { get; set; }

  [JsonPropertyName("quantity")]
  public int Quantity { get; set; }

  [JsonPropertyName("product_id")]
  public int ProductId { get; set; }

  [JsonPropertyName("product")]
  public Product Product { get; set; }
}