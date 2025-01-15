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
    [Route("order")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<User> _userManager;
        public OrderController(IOrderService orderService, UserManager<User> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> CreateOrder(OrderDto orderDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var order = await _orderService.CreateOrderAsync(user, orderDto);

            return Ok(order.ToNewOrderDto());
        }

        [Authorize]
        [HttpDelete("cancel")]
        public async Task<IActionResult> DeleteOrder(int orderId)
        {
            await _orderService.DeleteOrderAsync(orderId);

            return NoContent();
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllOrdersByUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var orders = await _orderService.GetAllOrdersByUserAsync(userId);
            var ordersDto = orders.Select(o => o.ToNewOrderDto());


            return Ok(ordersDto);
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);

            return Ok(order.ToNewOrderDto());
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateOrder(int orderId, OrderDto orderDto)
        {
            var order = await _orderService.UpdateOrderAsync(orderId, orderDto);

            return Ok(order.ToNewOrderDto());
        }

    }
}