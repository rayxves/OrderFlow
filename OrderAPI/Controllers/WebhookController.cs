
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

                if (session != null)
                {
                    if (session.PaymentStatus == "paid")
                    {
                        var paymentIntentService = new PaymentIntentService();
                        var paymentIntent = paymentIntentService.Get(session.PaymentIntentId);

                        var paymentDto = await _paymentsService.GetPaymentData(session.Id);

                        if (paymentDto != null)
                        {
                            await _paymentsService.onPaymentSuccess(paymentDto.User, paymentDto.Order);
                            return Ok("Pagamento processado com sucesso!");
                        }
                        else
                        {
                            return BadRequest("Erro ao processar o pagamento.");
                        }
                    }
                    else if (session.PaymentStatus == "unpaid" || session.PaymentStatus == "requires_payment_method")
                    {
                        var paymentFail = await _paymentsService.GetPaymentData(session.Id);
                        await _paymentsService.onPaymentFailure(paymentFail.User, paymentFail.Order);
                        return BadRequest("Pagamento falhou ou cartão recusado.");
                    }
                    else if (session.PaymentStatus == "requires_action")
                    {
                        return BadRequest("Ação adicional necessária para completar o pagamento.");
                    }
                    else
                    {
                        return BadRequest("Status de pagamento desconhecido.");
                    }
                }

                return BadRequest("Sessão de pagamento não encontrada.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar o pagamento: {ex.Message}");
                return StatusCode(500, "Erro interno ao processar o pagamento.");
            }
        }

    }
}
