using System;
using ProductClient.Models;

namespace ProductClient.Interfaces;

public interface IProductService
{
    Task<List<ProductDto>> GetAllProductsAsync();
    Task<ProductDto> GetProductsByNameAsync(string name);
}
