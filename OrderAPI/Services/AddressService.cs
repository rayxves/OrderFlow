using Microsoft.EntityFrameworkCore;
using OrderAPI.Data;
using OrderAPI.Dtos;
using OrderAPI.Interfaces;
using OrderAPI.Models;

namespace OrderAPI.Services
{
    public class AddressService : IAddressService
    {
        private readonly ApplicationDBContext _context;
        public AddressService(ApplicationDBContext context)
        {
            _context = context;
        }
        public async Task<Address> CreateAdressAsync(User user, AddressDto addressDto)
        {
            var existsUser = await _context.Users.FindAsync(user.Id);
            if (existsUser == null)
            {
                throw new InvalidOperationException("User not found!");
            }

            var newAddress = new Address
            {
                UserId = user.Id,
                User = user,
                Street = addressDto.Street,
                Neighborhood = addressDto.Neighborhood,
                City = addressDto.City,
                State = addressDto.State,
                PostalCode = addressDto.PostalCode,
                Country = addressDto.Country,
            };

            _context.Addresses.Add(newAddress);
            await _context.SaveChangesAsync();

            return newAddress;
        }

        public async Task<Address> DeleteAddressAsync(int addressId)
        {
            var existsAddress = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId);
            if (existsAddress == null)
            {
                throw new InvalidOperationException("Address not found!");
            }

            _context.Addresses.Remove(existsAddress);
            await _context.SaveChangesAsync();
            return existsAddress;
        }

        public async Task<Address> GetAddressByIdAsync(int addressId)
        {
            var address = await _context.Addresses.FindAsync(addressId);
            if (address == null)
            {
                throw new InvalidOperationException("Address not found!");
            }
            return address;
        }

        public async Task<List<Address>> GetAllAddressByUser(string userId)
        {
            return await _context.Addresses.Where(a => a.UserId == userId).ToListAsync();
        }

        public async Task<Address> UpdateAddressAsync(int addressId, UpdateAddressDto addressDto)
        {
            var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId);
            if (address == null)
            {
                throw new InvalidOperationException("Address not found!");
            }

            address.Street = addressDto.Street;
            address.Neighborhood = addressDto.Neighborhood;
            address.City = addressDto.City;
            address.State = addressDto.State;
            address.PostalCode = addressDto.PostalCode;
            address.Country = addressDto.Country;

            await _context.SaveChangesAsync();
            return address;
        }
    }
}