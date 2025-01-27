using System.Net.Security;
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
            bool allItemsUpdatedSuccessfully = true;

            var rabbitMqUrl = _config["RabbitMQ:Url"];
            if (string.IsNullOrEmpty(rabbitMqUrl))
            {
                throw new InvalidOperationException("RabbitMQ URL is not configured. Please check your appsettings.");
            }

            var factory = new ConnectionFactory
            {
                Uri = new Uri(rabbitMqUrl),
                Port = 5672  //porta padrão para comunicação sem SSL
            };

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();
            Console.WriteLine("Connected to RabbitMQ");
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
                Console.WriteLine($" [x] Received {Encoding.UTF8.GetString(body)}");

                try

                {
                    var orderResponse = JsonSerializer.Deserialize<OrderResponse>(message);

                    foreach (var item in orderResponse.OrderItems)
                    {
                        Console.WriteLine(item.ProductId);
                        Console.WriteLine(item.Quantity);
                        var inventory = await _inventoryService.UpdateInventoryAsync(item.ProductId, item.Quantity);
                        if (inventory == null)
                        {
                            allItemsUpdatedSuccessfully = false;
                            throw new InvalidOperationException("Erro ao atualizar o inventário. Pagamento não confirmado.");
                        }

                    }

                    await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                    await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            await channel.BasicConsumeAsync(queue: "inventory_update", consumer: consumer, autoAck: false);
            await Task.Delay(Timeout.Infinite);
        }


    }
}
