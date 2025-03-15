using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;



namespace Services
{
    public class RabbitMqService
    {
        public async Task<int> SendMessage(string message)
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            await channel.QueueDeclareAsync(
                queue: "hello", durable: false, exclusive: false, autoDelete: false, arguments: null);

            string jsonString = JsonSerializer.Serialize(new
            {
                message = message,
                userid = 1
            });

            var body = Encoding.UTF8.GetBytes(jsonString);

            await channel.BasicPublishAsync(exchange: string.Empty, routingKey: "hello", body: body);
            Console.WriteLine($" [x] Sent {message}");

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
            consumer.ReceivedAsync += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" [x] Received {message}");
                return Task.CompletedTask;
            };

            await channel.BasicConsumeAsync("hello", autoAck: true, consumer: consumer);

            return 0;
        }
    }
}