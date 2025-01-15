using ProductClient.Models;

namespace ProductClient.Mappers;

public static class ProductMapper
{
    public static ProductDto ToProductDto(this Product product)
    {
        if (product == null)
        {
            throw new ArgumentNullException(nameof(product));
        }

        return new ProductDto
        {
            Id = product.Id,
            Title = product.Title,
            Description = product.Description,
            Price = product.Price,
            Category = product.Category,
            Image = product.Image,
            Rating = product.Rating,
        };
    }
}