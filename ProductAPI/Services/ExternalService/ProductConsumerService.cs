using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
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
        private IConnection _connection;
        private IChannel _channel;

        public ProductConsumerService(IConfiguration config, IInventoryService inventoryService)
        {
            _config = config;
            _inventoryService = inventoryService;
        }

        public async Task InitializeAsync()
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(_config["RabbitMQ:Url"]),
                Port = 5672
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
        }

        public async Task StartConsumingAsync()
        {

            Console.WriteLine("Connected to RabbitMQ");

            await _channel.QueueDeclareAsync(queue: "inventory_update", durable: true, exclusive: false, autoDelete: false, arguments: null);

            var responseQueue = "response_queue";
            await _channel.QueueDeclareAsync(queue: responseQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);

            Console.WriteLine("Filas criadas");

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" [x] Received {message}");
                var responseQueue = ea.BasicProperties.ReplyTo;
                var correlationId = ea.BasicProperties.CorrelationId;

                try
                {
                    var orderResponse = JsonSerializer.Deserialize<OrderResponse>(message);
                    var inventory = await _inventoryService.UpdateInventoryWithTransactionAsync(orderResponse.OrderItems);
                    var responseMessage = inventory ? "success" : "error";
                    var responseBody = Encoding.UTF8.GetBytes(responseMessage);

                    var responseProperties = new BasicProperties { CorrelationId = correlationId };

                    Console.WriteLine($" [x] Sending response: {responseMessage}");
                    await _channel.BasicPublishAsync(
                        exchange: "",
                        routingKey: responseQueue,
                        basicProperties: responseProperties,
                        body: responseBody,
                        mandatory: true);

                    await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing message: {ex.Message}");
                    await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false);
                }
            };

            await _channel.BasicConsumeAsync(queue: "inventory_update", consumer: consumer, autoAck: false);
            await Task.Delay(Timeout.Infinite);
        }


        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
