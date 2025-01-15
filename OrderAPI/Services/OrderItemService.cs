using System;
using OrderAPI.Data;
using OrderAPI.Dtos;
using OrderAPI.Interfaces;
using OrderAPI.Models;
using ProductClient.Interfaces;

namespace OrderAPI.Services;

public class OrderItemService : IOrderItemService
{
    private readonly ApplicationDBContext _context;
    private readonly IProductService _productService;
    public OrderItemService(ApplicationDBContext context, IProductService productService)
    {
        _context = context;
        _productService = productService;
    }
    public async Task<OrderItem> CreateOrderItemAsync(OrderItemDto orderItemDto, Order order)
    {
        if (orderItemDto.ProductName == null || orderItemDto.Quantity == 0)
        {
            throw new ArgumentException("Invalid order item data!");
        }

        var product = await _productService.GetProductsByNameAsync(orderItemDto.ProductName);

        if (product == null)
        {
            throw new InvalidOperationException($"No product found with the name {orderItemDto.ProductName}");
        }

        var orderItem = new OrderItem
        {
            Order = order,
            OrderId = order.Id,
            ProductName = product.Title,
            Price = product.Price,
            Quantity = orderItemDto.Quantity
        };

        return orderItem;
    }
}
