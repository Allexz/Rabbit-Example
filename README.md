# **Integração entre: WEBAPI\C# e RABBITMQ**  

### Docker
---
O comando abaixo inicia um contêiner RABBITMQ com o plugin de gerenciamento, mapeando várias portas para diferentes serviços e funcionalidades do RABBITMQ.  
```docker run -d --name rabbitmq -p 4369:4369 -p 5551:5551 -p 5552:5552 -p 5671:5671 -p 5672:5672 -p 15671:15671 -p 15672:15672  rabbitmq:3-management```  
#### Detalhamento do comando:  
1. **docker run** - Comando base para execução de um contêiner DOCKER;
2. **-d** - Executa o contêiner em segundo plano (modo _"detached"_);
3. **-p** - Mapeia as portas do contêiner para as portas do HOST;
4. **rabbitmq:3-management** - Especifica a imagem DOCKER a ser usada. Neste caso a imagem do RABBITMQ com o plugin de gerenciamento habilitado.

---

### .NET\C#  
---  
Para este exemplo, utilizei dois projetos, um do tipo **CONSOLE** e outro do tipo **WEBAPI** .
A especificidade se resume ao detalhe da implementação do RABBITMQ em cada um dos projetos.  
A **WEBAPI** envia uma requisição para a aplicação **CONSOLE** através da execução do método abaixo:  
```
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
```

    
