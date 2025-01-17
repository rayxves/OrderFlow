using Microsoft.AspNetCore.Mvc;
using Stripe;


namespace OrderAPI.Controllers
{
    [Route("webhook")]
    [ApiController]
    public class WebhookController
    {
        private readonly IConfiguration _config;
        public WebhookController(IConfiguration config)
        {
            _config = config;
            StripeConfiguration.ApiKey = _config["Stripe:ApiKey"];
        }

        
        // [HttpPost]
        // public async Task<IActionResult> TestStripeWebhook([FromBody]){

        // }
    }
}