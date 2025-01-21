using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using OrderAPI.Interfaces;
using Stripe;
using Stripe.Checkout;

namespace OrderAPI.Controllers
{
    [Route("webhook")]
    [ApiController]
    public class WebhookController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IPaymentService _paymentsService;
        public WebhookController(IConfiguration config, IPaymentService paymentsService)
        {
            _config = config;
            _paymentsService = paymentsService;

        }

        [HttpGet]
        public async Task<IActionResult> HandleSuccessRedirect([FromQuery] string session_id)
        {
            if (string.IsNullOrEmpty(session_id))
            {
                return BadRequest("Session ID is required.");
            }

            try
            {
                StripeConfiguration.ApiKey = _config["Stripe:ApiKey"];
                var service = new SessionService();

                var session = await service.GetAsync(session_id);

                if (session != null && session.PaymentStatus == "paid")
                {
                    var paymentIntentService = new PaymentIntentService();
                    var paymentIntent = paymentIntentService.Get(session.PaymentIntentId);

                    var paymentDto = await _paymentsService.GetPaymentSuccessData(session.Id);

                    if (paymentDto != null)
                    {
                        Console.WriteLine(paymentDto.User.ToString());
                        Console.WriteLine(paymentDto.Order.ToString());
                        await _paymentsService.onPaymentSuccess(paymentDto.User, paymentDto.Order);
                        Console.WriteLine($"Pagamento bem-sucedido para a sessão: {session.Id}");
                        return Ok("Pagamento processado com sucesso!");
                    }
                    else
                    {
                        return BadRequest("Erro ao processar o pagamento.");
                    }
                }

                return BadRequest("Sessão de pagamento não foi concluída com sucesso.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar o pagamento: {ex.Message}");
                return StatusCode(500, "Erro interno ao processar o pagamento.");
            }
        }
    }
}
