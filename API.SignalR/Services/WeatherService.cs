using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace WeatherForecastApp.API.Services
{
    public class WeatherService:BackgroundService
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly EventingBasicConsumer consumer;
        public WeatherService()
        {

            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (connection = factory.CreateConnection())
            using (channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello",
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                consumer = new EventingBasicConsumer(channel);
            }
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            consumer.Received += async (model, content) =>
            {
                var body = content.Body.ToArray();
                var result = Encoding.UTF8.GetString(body);
                var props = content.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                result = result.Replace("\"", "");
                Console.WriteLine(" [x] Received {0}", result);
                using (var httpClient = new HttpClient())
                {
                    var URL = "http://localhost:59165/weatherforecast/" + result;
                    Console.WriteLine(URL);
                    var response = await httpClient.GetStringAsync($"{URL}");
                    Thread.Sleep(1000);
                    Console.WriteLine(response);
                    
                    replyProps.CorrelationId = props.CorrelationId;
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    channel.BasicPublish(
                        exchange: "",                // Must reply to default exchange ("")
                        routingKey: props.ReplyTo,
                        basicProperties: replyProps,
                        body: responseBytes);
                    channel.BasicAck(deliveryTag: content.DeliveryTag, multiple: false);
                }
            };

            channel.BasicConsume("hello", true, consumer);
            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}