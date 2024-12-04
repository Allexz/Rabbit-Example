# **Integração entre: WEBAPI\C# e RABBITMQ**  

### Docker  
O comando abaixo inicia um contêiner RABBITMQ com o plugin de gerenciamento, mapeando várias portas para diferentes serviços e funcionalidades do RABBITMQ.  
```docker run -d --name rabbitmq -p 4369:4369 -p 5551:5551 -p 5552:5552 -p 5671:5671 -p 5672:5672 -p 15671:15671 -p 15672:15672  rabbitmq:3-management```  
#### Detalhamento do comando:  
1. **docker run** - Comando base para execução de um contêiner DOCKER;
2. **-d** - Executa o contêiner em segundo plano (modo _"detached"_);
3. **-p** - Mapeia as portas do contêiner para as portas do HOST;
4. **rabbitmq:3-management** - Especifica a imagem DOCKER a ser usada. Neste caso a imagem do RABBITMQ com o plugin de gerenciamento habilitado.

---

### .NET\C#  
Para este exemplo, utilizei dois projetos, um do tipo **CONSOLE** e outro do tipo **WEBAPI** .
A especificidade se resume ao detalhe da implementação do RABBITMQ em cada um dos projetos.  
A mensagem é publicada por intermédio de uma requisição feita à **WEBAPI** através da execução do método abaixo:  
```
public class RabbitMQProducer : IRabbitMQProducer
{
    public async Task SendProductMessage<T>(T message)
    {
        //Cria uma *Connection Factory*  para configuracao da conexao ao RABBITMQ Server especificando, inclusive, o nome do servidor.
        ConnectionFactory? factory = new ConnectionFactory { HostName = "localhost" };

        //Estabelece a conexao com o servidor.
        using IConnection? connection = await factory.CreateConnectionAsync();

        //Cria o canal, uma conexao virtual estabelecida sobre a conexao ja criada.
        using IChannel? canal = await connection.CreateChannelAsync();

        //Declara a fila e seus parametros
        await canal.QueueDeclareAsync(queue: "ProductsQueue",
                              durable: false,
                              exclusive: false,
                              autoDelete: false,
                              arguments: null);

        //Serializacao da mensagem
        string json = JsonSerializer.Serialize(message);
        byte[]? body = Encoding.UTF8.GetBytes(json);

        //Mensagem publicada
        await canal.BasicPublishAsync(exchange: string.Empty,
                                      routingKey: "ProductsQueue",
                                      body: body,
                                      CancellationToken.None);
    }
}
```
---
A aplicação ***CONSOLE*** _"fica escutando"_ as mensagens publicadas através da execução do método abaixo:  
```
//Observe que esta primeira parte do codigo e igual a do metodo de publicacao.
//Cabe salientar que a fila nao sera criada novamente
ConnectionFactory? factory = new ConnectionFactory { HostName = "localhost" };
using IConnection? connection = await factory.CreateConnectionAsync();
using IChannel? canal = await connection.CreateChannelAsync();

await canal.QueueDeclareAsync(queue: "ProductsQueue",
                              durable: false,
                              exclusive: false,
                              autoDelete: false,
                              arguments: null);

Console.WriteLine("Aguardando novas mensagens");

//Cria o *consumidor* destinado a capturar as mensagens publicadas
AsyncEventingBasicConsumer? consumer = new AsyncEventingBasicConsumer(canal);

//Delegate que sera acionado sempre que uma nova mensagem e recebida
consumer.ReceivedAsync += async (model, ea) =>
{
    try
    {
        //Desserializacao da mensagem
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        Console.WriteLine($" [x] Received {message}");

        //Sinaliza o recebimento da mensagem e executa o processamento para o RABBITMQ Server
        await canal.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
    }
    catch (Exception)
    {
        //Tratamento de erro que ira repor a mensagem - **requeue: true** - na fila novamente.
        //Convem adequar estrategia para evitar LOOPS infinitos e documentacao do processo
        await canal.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
    }
};

```
