using Microsoft.AspNetCore.Mvc;
using ProductAPI.Dtos;
using ProductAPI.Interfaces;

namespace ProductAPI.Controllers
{
    [ApiController]
    [Route("inventory")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        [HttpPut]
        public async Task<IActionResult> UpdateInventory(IEnumerable<OrderItemResponse> orderItems)
        {
            if (!ModelState.IsValid) throw new ArgumentException("Invalid order format.");
            var inventoryUpdated = await _inventoryService.AddToInventoryAsync(orderItems);

            if (inventoryUpdated)
            {
                return Ok("Inventory updated successfully");
            }
            else
            {
                return BadRequest("Failed to update inventory");
            }
        }
    }
}