
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrderAPI.Interfaces;
using OrderAPI.Mappers;
using OrderAPI.Models;


namespace OrderAPI.Controllers
{
    [Route("delivery")]
    [ApiController]
    public class DeliveryController : ControllerBase
    {
        private readonly IDeliveryService _deliveryService;
        private readonly UserManager<User> _userManager;
        public DeliveryController(IDeliveryService deliveryService, UserManager<User> userManager)
        {
            _deliveryService = deliveryService;
            _userManager = userManager;
        }

        [HttpGet("by-user")]
        public async Task<IActionResult> GetALLDeliveriesByUserAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated");
            }

            var deliveries = await _deliveryService.GetDeliveriesByUserAsync(userId);

            return Ok(deliveries.Select(d => d.ToDeliveryDto()));
        }
    }
}