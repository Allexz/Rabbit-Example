namespace WebApiRabbitMQ.Services;

public interface IRabbitMQProducer
{
    Task SendProductMessage<T>(T message);
}
