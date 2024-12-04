using Microsoft.AspNetCore.Mvc;
using WebApiRabbitMQ.Model;
using WebApiRabbitMQ.Services;

namespace WebApiRabbitMQ.Controllers;
[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly IRabbitMQProducer _producer;
    public ProductController(IRabbitMQProducer producer)
    {
        _producer = producer;
    }
    
    [HttpPost]
    public IActionResult CreateProduct([FromBody] Product product)
    {
        _producer.SendProductMessage(product);
        return Ok("Produto enviado para a fila.");
    }
}
