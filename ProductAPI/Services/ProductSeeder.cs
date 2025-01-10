using ProductAPI.Data;
using ProductAPI.Interfaces;
using ProductAPI.Models;

namespace ProductAPI.Services
{
    public class ProductSeeder : IProductSeeder
    {
        private readonly ApplicationDBContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public ProductSeeder(ApplicationDBContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        public async Task SeedAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://fakestoreapi.com/products");
            if (response.IsSuccessStatusCode)
            {
                var products = await response.Content.ReadFromJsonAsync<List<Product>>();
                if (products != null && products.Any())
                {
                    foreach (var product in products)
                    {
                        if (!_context.Products.Any(p => p.Id == product.Id))
                        {
                            _context.Products.Add(product);
                        }
                    }
                    await _context.SaveChangesAsync();
                }
            }
        }

    }
}