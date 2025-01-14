using ProductAPI.Dtos;
using ProductAPI.Models;

namespace ProductAPI.Interfaces
{
    public interface IProductService
    {
        Task<List<Product>> GetProductsAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task<List<Product>> GetProductsByNameAsync(string name);
        Task<Product> CreateProductAsync(ProductDto productDto);
        Task<Product> UpdateProductAsync(int id, ProductDto productDto);
        Task<Product> DeleteProductAsync(int id);
    }
}
