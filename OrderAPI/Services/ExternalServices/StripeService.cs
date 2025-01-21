
using Stripe;

using Stripe.Checkout;

namespace OrderAPI.Services
{
    public class StripeService
    {
        private readonly IConfiguration _config;

        public StripeService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<Session> CreateCheckoutSessionAsync(List<SessionLineItemOptions> lineItems)
        {
            StripeConfiguration.ApiKey = _config["Stripe:ApiKey"];

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = $"{_config["Stripe:SuccessUrl"]}?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{_config["Stripe:CancelUrl"]}?session_id={{CHECKOUT_SESSION_ID}}"
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return session;
        }


    }
}