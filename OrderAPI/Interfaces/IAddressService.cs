using OrderAPI.Dtos;
using OrderAPI.Models;

namespace OrderAPI.Interfaces
{
    public interface IAddressService
    {
        Task<Address> CreateAdressAsync(User user, AddressDto addressDto);
        Task<List<Address>> GetAllAddressByUser(string userId);
        Task<Address> UpdateAddressAsync(int addressId, UpdateAddressDto addressDto);
        Task<Address> GetAddressByIdAsync(int addressId);
        Task<Address> DeleteAddressAsync(int addressId);
    }
}