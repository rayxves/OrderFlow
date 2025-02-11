using Microsoft.EntityFrameworkCore;
using OrderAPI.Data;
using OrderAPI.Interfaces;
using OrderAPI.Models;

namespace OrderAPI.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly ApplicationDBContext _context;
        private readonly GoogleAPIService _googleService;
        private readonly IConfiguration _config;
        public DeliveryService(ApplicationDBContext context, GoogleAPIService googleService, IConfiguration config)
        {
            _context = context;
            _googleService = googleService;
            _config = config;
        }
        public async Task<Delivery> CreateDeliveryAsync(int orderId, int addressId)
        {
            if (orderId == 0 || addressId == 0)
            {
                throw new ArgumentException("Both orderId and addressId must be provided", nameof(orderId));
            }


            var address = await _context.Addresses.FindAsync(addressId);
            var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);

            if (address == null)
            {
                throw new InvalidOperationException("Address not found!");
            }

            if (order == null)
            {
                throw new InvalidOperationException("Order not found!");
            }

            var myAddress = _config["MyAddress:Address"];

            if (string.IsNullOrEmpty(myAddress))
            {
                throw new InvalidOperationException("MyAddress:Address configuration is missing or empty.");
            }

            var deliveryAddress = $"{address.Street}, {address.City}, {address.State}, {address.Country}";

            var response = await _googleService.GetDeliveryTimeAsync(myAddress, deliveryAddress);
            if (response.Status == "OK")
            {
                var maxPreparationTime = _config.GetValue<int>("Delivery:MaxPreparationTime");
                var fromArrivalToDeliveryTime = _config.GetValue<int>("Delivery:FromArrivalToDeliveryTime");
                var totalTime = maxPreparationTime + fromArrivalToDeliveryTime;

                var deliveryDate = DateTime.UtcNow.AddSeconds(response.Rows[0].Elements[0].Duration.Value).AddSeconds(totalTime);

                var delivery = new Delivery
                {
                    OrderId = orderId,
                    Order = order,
                    AddressId = addressId,
                    Address = address,
                    DeliveryDate = deliveryDate,
                    Status = "Pending"
                };

                _context.Deliveries.Add(delivery);
                await _context.SaveChangesAsync();

                return delivery;
            }

            throw new Exception("Erro ao calcular o tempo de entrega. Verifique se o endereço de origem e destino estão corretos ou se a API de cálculo de tempo de entrega está acessível.");

        }

        public async Task<List<Delivery>> GetDeliveriesByDateAndOrderSuccess(DateTime date)
        {
            date = date.ToUniversalTime();
            return await _context.Deliveries.Include(d => d.Order).Include(d => d.Address).ThenInclude(a => a.User).Where(d => d.DeliveryDate <= date && d.Order.Status == "Payment Success" && d.Status == "Shipped").ToListAsync();
        }

        public async Task<List<Delivery>> GetDeliveriesByUserAndStatusAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            var deliveries = await _context.Deliveries.Where(d => d.Address.UserId == userId && d.Order.Status == "Payment Success").ToListAsync();
            if (!deliveries.Any())
            {
                throw new InvalidOperationException("No deliveries found for this user!");
            }
            return deliveries;
        }

        public async Task<List<Delivery>> GetDeliveriesByUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty.", nameof(userId));
            }

            var deliveries = await _context.Deliveries.Include(d => d.Address).Include(d => d.Order).Where(d => d.Address.UserId == userId).ToListAsync();
            if (!deliveries.Any())
            {
                throw new InvalidOperationException("No deliveries found for this user!");
            }
            return deliveries;
        }
    }
}