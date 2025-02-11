using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OrderAPI.Services
{
    public class RabbitMqService : IDisposable
    {
        private readonly IConfiguration _config;
        private IConnection _connection;
        private IChannel _channel;
        private AsyncEventingBasicConsumer _consumer;
        private bool _isInitialized;
        private TaskCompletionSource<bool> _responseCompletionSource;

        public RabbitMqService(IConfiguration config)
        {
            _config = config;
        }

        public async Task InitializeAsync()
        {
            if (_isInitialized) return;

            var factory = new ConnectionFactory
            {
                Uri = new Uri(_config["RabbitMQ:Url"]),
                Port = 5672
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();


            if (_consumer == null)
            {
                _consumer = new AsyncEventingBasicConsumer(_channel);
            }

            _consumer.ReceivedAsync += async (sender, args) =>
            {
                var correlationId = args.BasicProperties.CorrelationId;
                Console.WriteLine($"Mensagem recebida: CorrelationId = {correlationId}");

                var responseBody = Encoding.UTF8.GetString(args.Body.ToArray());
                Console.WriteLine($"Mensagem: {responseBody}");

                try
                {
                    if (_responseCompletionSource != null && !_responseCompletionSource.Task.IsCompleted)
                    {
                        bool success = responseBody == "success";
                        _responseCompletionSource.SetResult(success);
                    }

                    await _channel.BasicAckAsync(args.DeliveryTag, false);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erro ao processar mensagem: {ex.Message}");
                    await _channel.BasicNackAsync(args.DeliveryTag, false, true);
                }
            };

            _isInitialized = true;

            Console.WriteLine("Conectando ao RabbitMQ e registrando consumidor");
        }

        public async Task<bool> SendMessageAsync<T>(string queueName, T message)
        {
            await InitializeAsync();

            var responseQueue = "response_queue";
            var correlationId = Guid.NewGuid().ToString();

            await _channel.QueueDeclareAsync(responseQueue, durable: true, exclusive: false, autoDelete: false, arguments: null);
            await _channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var basicProperties = new BasicProperties
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
                Persistent = true,
                MessageId = Guid.NewGuid().ToString(),
                CorrelationId = correlationId,
                ReplyTo = responseQueue,
            };

            var messageBody = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(messageBody);

            _responseCompletionSource = new TaskCompletionSource<bool>();

            if (_consumer != null)
            {
                var consumerTag = await _channel.BasicConsumeAsync(queue: responseQueue, autoAck: false, consumer: _consumer);
                Console.WriteLine($"[x] Consumidor registrado com tag: {consumerTag}");
            }

            await _channel.BasicPublishAsync(exchange: "", routingKey: queueName, basicProperties: basicProperties, body: body, mandatory: true);

            return await _responseCompletionSource.Task;
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
