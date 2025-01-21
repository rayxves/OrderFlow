using System;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using System.Threading.Tasks;
using System.Net.Security;

namespace OrderAPI.Services
{
    public class RabbitMqService
    {
        private readonly IConfiguration _config;

        public RabbitMqService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendMessageAsync<T>(string queueName, T message)
        {
            var rabbitMqUrl = _config["RabbitMQ:Url"];
            Console.WriteLine($"RabbitMQ URL: {rabbitMqUrl}");

            if (string.IsNullOrEmpty(rabbitMqUrl))
            {
                Console.WriteLine("No RabbitMQ URL specified");
                throw new InvalidOperationException("RabbitMQ URL is not configured. Please check your appsettings.");
            }

            Console.WriteLine("Entrou aqui");

            var factory = new ConnectionFactory
            {
                Uri = new Uri(rabbitMqUrl),
                Port = 5672
            };

            Console.WriteLine("Saíu aqui");

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            var basicProperties = new BasicProperties
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
                Persistent = true,
                MessageId = Guid.NewGuid().ToString(),
                CorrelationId = Guid.NewGuid().ToString()
            };

            await channel.QueueDeclareAsync(queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var messageBody = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(messageBody);

            await channel.BasicPublishAsync(exchange: "",
                routingKey: queueName,
                basicProperties: basicProperties,
                body: body,
                mandatory: true);

            Console.WriteLine($" [x] Sent {queueName}: {messageBody}");
        }
    }
}
