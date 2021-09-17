using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace API.RabbitMQ.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RabbitController : ControllerBase
    {
        private readonly ILogger<RabbitController> _logger;

        public RabbitController(ILogger<RabbitController> logger)
        {
            _logger = logger;
        }

        [HttpPost("/message")]
        public ActionResult<string> Post([FromQuery] string message)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "rabbit",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
                
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                    routingKey: "rabbit",
                    basicProperties: null,
                    body: body);
            }
            return Ok($"Message {message} has been published");
        }
    }
}
