using Microsoft.AspNetCore.Mvc;
using ProductClient.Interfaces;

namespace OrderAPI.Controllers
{
    [Route("products")]
    [ApiController]
    public class ProductController : ControllerBase{
        private readonly IProductService _productService;
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts(){
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpGet("by-name")]
        public async Task<IActionResult> GetProductByName(string name){
            var products = await _productService.GetProductsByNameAsync(name);
            return Ok(products);
        }
    }
}