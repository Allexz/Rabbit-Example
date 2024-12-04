using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace WebApiRabbitMQ.Services;

public class RabbitMQProducer : IRabbitMQProducer
{
    public async Task SendProductMessage<T>(T message)
    {
        ConnectionFactory? factory = new ConnectionFactory { HostName = "localhost" };
        using IConnection? connection = await factory.CreateConnectionAsync();
        using IChannel? canal = await connection.CreateChannelAsync();

        await canal.QueueDeclareAsync(queue: "ProductsQueue",
                              durable: false,
                              exclusive: false,
                              autoDelete: false,
                              arguments: null);

        string json = JsonSerializer.Serialize(message);
        byte[]? body = Encoding.UTF8.GetBytes(json);

        await canal.BasicPublishAsync(exchange: string.Empty,
                                      routingKey: "ProductsQueue",
                                      body: body,
                                      CancellationToken.None);
    }
}
