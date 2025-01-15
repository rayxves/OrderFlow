using System;
using System.Text.Json.Serialization;

namespace ProductClient.Models;

public class Rating
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("rate")]
    public decimal Rate { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; }
}