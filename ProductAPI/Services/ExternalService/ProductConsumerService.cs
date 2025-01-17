using System.Text;
using System.Text.Json;
using ProductAPI.Dtos;
using ProductAPI.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ProductAPI.Services
{
    public class ProductConsumerService
    {
        private readonly IConfiguration _config;
        private readonly IInventoryService _inventoryService;

        public ProductConsumerService(IConfiguration config, IInventoryService inventoryService)
        {
            _config = config;
            _inventoryService = inventoryService;
        }

        public async Task StartConsumingAsync()
        {
            var factory = new ConnectionFactory
            {
                HostName = _config["RabbitMQ:Host"],
                UserName = _config["RabbitMQ:Username"],
                Password = _config["RabbitMQ:Password"]
            };

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: "inventory_update",
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var consumer = new AsyncEventingBasicConsumer(channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                try
                {
                    var productUpdateMessage = JsonSerializer.Deserialize<ProductUpdateMessage>(message);

                    var inventory = await _inventoryService.UpdateInventoryAsync(productUpdateMessage.ProductId, productUpdateMessage.Quantity);
                    Console.WriteLine($"Updated Inventory: {inventory.ProductId} - {inventory.Quantity}");

                    await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                    await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            await channel.BasicConsumeAsync(queue: "inventory_update", consumer: consumer, autoAck: false);
        }
    }
}
