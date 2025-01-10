using ProductAPI.Models;

namespace ProductAPI.Interfaces
{
    public interface IProductService
    {
        Task<List<Product>> GetProductsAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(Product product);
        Task<Product> UpdateProductAsync(int id, Product product);
        Task<Product> DeleteProductAsync(int id);
    }
}