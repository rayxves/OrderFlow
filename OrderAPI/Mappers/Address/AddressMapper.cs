using OrderAPI.Dtos;
using OrderAPI.Models;

namespace OrderAPI.Mappers
{
    public static class AddressMapper
    {
        public static AddressDto ToAddressDto(this Address address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address));
            }

            return new AddressDto
            {
                Id = address.Id,
                Street = address.Street,
                Neighborhood = address.Neighborhood,
                City = address.City,
                State = address.State,
                PostalCode = address.PostalCode,
                Country = address.Country
            };
        }
    }
}