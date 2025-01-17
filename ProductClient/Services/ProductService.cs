using System.Net.Http.Json;
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
        try
        {
            var response = await _httpClient.GetAsync("/products");
            response.EnsureSuccessStatusCode();
            var products = await response.Content.ReadFromJsonAsync<List<Product>>();
            return products?.Select(p => p.ToProductDto()).ToList() ?? new List<ProductDto>();
        }
        catch (HttpRequestException e)
        {
            throw new Exception($"Error fetching products: {e.Message}", e);
        }
        catch (JsonException e)
        {
            throw new Exception("Error parsing product data", e);
        }
    }

    public async Task<ProductDto> GetProductsByNameAsync(string name)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/products/by-name?name={Uri.EscapeDataString(name)}");
            response.EnsureSuccessStatusCode();
            var product = await response.Content.ReadFromJsonAsync<Product>();
            return product?.ToProductDto() ?? throw new Exception("Product not found");
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception($"Product with name '{name}' not found", e);
            }
            throw new Exception($"Error fetching product '{name}': {e.Message}", e);
        }
        catch (JsonException e)
        {
            throw new Exception($"Error parsing product data for '{name}'", e);
        }
    }

    public async Task<ProductDto> GetProductsByIdAsync(int id)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/products/?id={id}");
            response.EnsureSuccessStatusCode();
            var product = await response.Content.ReadFromJsonAsync<Product>();
            return product?.ToProductDto() ?? throw new Exception("Product not found");
        }
        catch (HttpRequestException e)
        {
            if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception($"Product with name '{id}' not found", e);
            }
            throw new Exception($"Error fetching product '{id}': {e.Message}", e);
        }
        catch (JsonException e)
        {
            throw new Exception($"Error parsing product data for '{id}'", e);
        }
    }
}