using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using Repositories.Models;

namespace Services
{
    public class RabbitMqService
    {
        private readonly RedisService _redis;

        public RabbitMqService(RedisService redisService) => _redis = redisService;

        public async Task<int> SendMessage(Notification message)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

            string jsonString = JsonSerializer.Serialize(message);

            var body = Encoding.UTF8.GetBytes(jsonString);

            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "hello", body: body);
            Console.WriteLine($" [x] Sent {jsonString}");

            return 0;
        }

        public async Task<int> ReadMessage()
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(queue: "hello", durable: false, exclusive: false, autoDelete: false,
                arguments: null);

            Console.WriteLine(" [*] Waiting for messages.");

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                string b = message;
                await _redis.SetStringAsync("Notifications", b);
                Console.WriteLine($" [x] Received {message}");
                // return System.Threading.Tasks.Task.CompletedTask;
            };

            await channel.BasicConsumeAsync("hello", autoAck: true, consumer: consumer);
            while (true) { Thread.Sleep(1000); }

            return 0;
        }
    }



    public class RabbitMqBackgroundService : BackgroundService
    {
        private readonly RabbitMqService _rabbitMqService;

        public RabbitMqBackgroundService(RabbitMqService rabbitMqService)
        {
            _rabbitMqService = rabbitMqService;
        }

        protected override System.Threading.Tasks.Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _rabbitMqService.ReadMessage();
            return System.Threading.Tasks.Task.CompletedTask;
        }
    }

}