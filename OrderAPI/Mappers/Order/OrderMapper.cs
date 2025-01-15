using OrderAPI.Dtos;
using OrderAPI.Models;

namespace OrderAPI.Mappers
{
    public static class OrderMapper
    {
        public static NewOrderDto ToNewOrderDto(this Order order)
        {

            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            return new NewOrderDto
            {
                Id = order.Id,
                Status = order.Status,
                OrderDate = DateTime.Now,
                Total = order.OrderItems.Sum(i => i.Price * i.Quantity),
                OrderItemsDto = order.OrderItems.Select(oi => oi.ToOrderItemDto()).ToList(),
            };
        }

        public static OrderItemDto ToOrderItemDto(this OrderItem orderItem)
        {
            if (orderItem == null)
            {
                throw new ArgumentNullException(nameof(orderItem));
            }

            return new OrderItemDto
            {
                ProductName = orderItem.ProductName,
                Quantity = orderItem.Quantity
            };

        }
    }
}