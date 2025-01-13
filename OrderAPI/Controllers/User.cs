using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace OrderAPI.Controllers
{
    [Route("user")]
    [ApiController]
    public class User : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        public User(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("/register")]
        public async Task<IActionResult> Register([FromBody]){

        }
    }
}