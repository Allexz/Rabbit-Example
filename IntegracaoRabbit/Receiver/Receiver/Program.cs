using RabbitMQ;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

ConnectionFactory? factory = new ConnectionFactory { HostName = "localhost" };
using IConnection? connection = await factory.CreateConnectionAsync();
using IChannel? canal = await connection.CreateChannelAsync();

await canal.QueueDeclareAsync(queue: "ProductsQueue",
                              durable: false,
                              exclusive: false,
                              autoDelete: false,
                              arguments: null);

Console.WriteLine("Aguardando novas mensagens");

AsyncEventingBasicConsumer? consumer = new AsyncEventingBasicConsumer(canal);
consumer.ReceivedAsync += async (model, ea) =>
{
    try
    {
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        Console.WriteLine($" [x] Received {message}");

        await canal.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
    }
    catch (Exception)
    {
        await canal.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
    }

};

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();