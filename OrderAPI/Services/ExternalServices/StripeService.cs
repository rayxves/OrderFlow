
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

        public async Task<string> CreateCheckoutSessionAsync(List<SessionLineItemOptions> lineItems)
        {
            StripeConfiguration.ApiKey = _config["Stripe:ApiKey"];

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = _config["Stripe:SuccessUrl"],
                CancelUrl = _config["Stripe:CancelUrl"],
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return session.Url;
        }


    }
}