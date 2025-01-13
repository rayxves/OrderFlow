using Microsoft.EntityFrameworkCore;
using ProductAPI.Data;
using ProductAPI.Dtos;
using ProductAPI.Interfaces;
using ProductAPI.Models;

namespace ProductAPI.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDBContext _context;
        public ProductService(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<Product> CreateProductAsync(ProductDto productDto)
        {
            if (productDto == null)
            {
                throw new ArgumentNullException(nameof(productDto));
            }

            var rating = new Rating
            {
                Rate = productDto.Rating.Rate,
                Count = productDto.Rating.Count
            };
            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            var product = new Product
            {
                Title = productDto.Title,
                Price = productDto.Price,
                Description = productDto.Description,
                Category = productDto.Category,
                Image = productDto.Image,
                Rating = rating
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var inventory = new Inventory
            {
                Quantity = productDto.Inventory.Quantity,
                ProductId = product.Id
            };

            _context.Inventories.Add(inventory);
            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<Product> DeleteProductAsync(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return null;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _context.Products.Include(p => p.Rating).Include(p => p.Inventories).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            return await _context.Products.Include(p => p.Rating).Include(p => p.Inventories).ToListAsync();
        }

        public async Task<Product> UpdateProductAsync(int id, ProductDto productDto)
        {
            var productToUpdate = await _context.Products.Include(p => p.Rating).Include(p => p.Inventories).FirstOrDefaultAsync(p => p.Id == id);
            if (productToUpdate == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found.");
            }

            productToUpdate.Rating.Rate = productDto.Rating.Rate;
            productToUpdate.Rating.Count = productDto.Rating.Count;

            productToUpdate.Title = productDto.Title;
            productToUpdate.Price = productDto.Price;
            productToUpdate.Description = productDto.Description;
            productToUpdate.Category = productDto.Category;
            productToUpdate.Image = productDto.Image;

            var inventory = productToUpdate.Inventories.FirstOrDefault();
            if (inventory != null)
            {
                inventory.Quantity = productDto.Inventory.Quantity;
            }
            else
            {
                var newInventory = new Inventory
                {
                    Quantity = productDto.Inventory.Quantity,
                    ProductId = productToUpdate.Id
                };
                _context.Inventories.Add(newInventory);
            }

            await _context.SaveChangesAsync();
            return productToUpdate;
        }
    }
}
