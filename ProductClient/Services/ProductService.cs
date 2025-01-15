using System.Text.Json;
using ProductClient.Interfaces;
using ProductClient.Mappers;
using ProductClient.Models;

namespace ProductClient;

public class ProductService : IProductService
{
    private readonly HttpClient _httpClient;
    public ProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<ProductDto>> GetAllProductsAsync()
    {
        var response = await _httpClient.GetAsync("/products");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        var products = JsonSerializer.Deserialize<List<Product>>(content);

        return products.Select(p => p.ToProductDto()).ToList();
    }

    public async Task<ProductDto> GetProductsByNameAsync(string name)
    {
        Console.WriteLine(name);

        var response = await _httpClient.GetAsync($"/products/by-name?name={name}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var products = JsonSerializer.Deserialize<Product>(content);

        return products.ToProductDto();
    }
};