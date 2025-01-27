using OrderAPI.Dtos;
using OrderAPI.Models;

namespace OrderAPI.Mappers
{
    public static class DeliveryMapper
    {
        public static DeliveryDto ToDeliveryDto(this Delivery delivery)
        {
            if (delivery == null)
            {
                throw new ArgumentNullException(nameof(delivery));
            }

            return new DeliveryDto
            {
                Id = delivery.Id,
                OrderId = delivery.OrderId,
                Status = delivery.Status,
                DeliveryDate = delivery.DeliveryDate,
                AddressId = delivery.AddressId,
            };
        }
    }
}