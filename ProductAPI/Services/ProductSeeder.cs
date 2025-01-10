using ProductAPI.Data;
using ProductAPI.Interfaces;
using ProductAPI.Models;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace ProductAPI.Services
{
    public class ProductSeeder : IProductSeeder
    {
        private readonly ApplicationDBContext _context;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProductSeeder> _logger;

        public ProductSeeder(ApplicationDBContext context, IHttpClientFactory httpClientFactory, ILogger<ProductSeeder> logger)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting data seeding...");

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync("https://fakestoreapi.com/products");

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Successfully fetched products from API.");
                var products = await response.Content.ReadFromJsonAsync<List<Product>>();
                if (products != null && products.Any())
                {
                    foreach (var product in products)
                    {
                        if (!_context.Products.Any(p => p.Id == product.Id))
                        {
                            _logger.LogInformation($"Seeding product: {product.Title}");

                            var rating = new Rating
                            {
                                Rate = product.Rating?.Rate ?? 0,
                                Count = product.Rating?.Count ?? 0
                            };

                            if (!_context.Ratings.Any(r => r.Id == product.RatingId))
                            {
                                _context.Ratings.Add(rating);
                                await _context.SaveChangesAsync();
                                _logger.LogInformation($"Added rating for product: {product.Title}");
                            }

                            var newProduct = new Product
                            {
                                Id = product.Id,
                                Title = product.Title,
                                Price = product.Price,
                                Description = product.Description,
                                Category = product.Category,
                                Image = product.Image,
                                RatingId = rating.Id
                            };

                            _context.Products.Add(newProduct);

                            var inventory = new Inventory
                            {
                                ProductId = newProduct.Id,
                                Quantity = GetRandomNumber(1, 101)
                            };
                            _context.Inventories.Add(inventory);

                            await _context.SaveChangesAsync();
                            _logger.LogInformation($"Added product and inventory: {product.Title}");
                        }
                    }
                }
                else
                {
                    _logger.LogError("Failed to fetch products from the API.");
                }
            }
        }

        private int GetRandomNumber(int minValue, int maxValue)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] randomNumber = new byte[4];
                rng.GetBytes(randomNumber);
                int value = BitConverter.ToInt32(randomNumber, 0);
                return new Random(value).Next(minValue, maxValue);
            }
        }
    }
}