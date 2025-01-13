using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrderAPI.Dtos;
using OrderAPI.Interfaces;
using OrderAPI.Mappers;
using OrderAPI.Models;

namespace OrderAPI.Controllers
{
    [Route("address")]
    [ApiController]
    public class AddressController : ControllerBase
    {
        private readonly IAddressService _addressService;
        private readonly UserManager<User> _userManager;
        public AddressController(IAddressService addressService, UserManager<User> userManager)
        {
            _addressService = addressService;
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAddressesByUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var addresses = await _addressService.GetAllAddressByUser(userId);
            if (addresses == null || !addresses.Any())
            {
                return NotFound("No addresses found for this user");
            }

            return Ok(addresses.Select(a => a.ToAddressDto()));
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> CreateAddress([FromBody] AddressDto addressDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = _userManager.Users.FirstOrDefault(u => u.Id == userId);

            var newAddress = await _addressService.CreateAdressAsync(user, addressDto);

            return Ok(newAddress.ToAddressDto());
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteAddress(int addressId)
        {
            await _addressService.DeleteAddressAsync(addressId);
            return NoContent();
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateAddress(int id, UpdateAddressDto addressDto)
        {
            var newAddress = await _addressService.UpdateAddressAsync(id, addressDto);
            return Ok("Successfully updated address");
        }
    }
}