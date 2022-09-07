using RabbitMQ.Client;
using System;
using System.Text;

namespace Producer
{
    internal class Send
    {
        static void Main()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using(var connection = factory.CreateConnection())
            {
                using(var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "hello",
                                         durable: false,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null); // Queue will only be created if it doesn't exist already.

                    string message = "Hello World!";

                    var body = Encoding.UTF8.GetBytes(message); // The message content is a byte array.

                    channel.BasicPublish(exchange: "", // Publishing the message into default (nameless) exchange.
                                         routingKey: "hello",
                                         basicProperties: null,
                                         body: body);

                    Console.WriteLine("Producer Sent {0}", message);
                }
            }

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
