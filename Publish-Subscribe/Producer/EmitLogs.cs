using RabbitMQ.Client;
using System;
using System.Text;

namespace EmitLog
{
    internal class EmitLog
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchange: "logs",
                                            type: ExchangeType.Fanout); // Exchange will only be created if it doesn't exist already.

                    string message = GetMessage(args);

                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "logs", // Publishing the message into 'logs' exchange.
                                         routingKey: "", // Since the Exchange Type is 'Fanout', it just broadcasts all the messages it receives to all the queues it knows. So no need to define routingKey.
                                         basicProperties: null,
                                         body: body);

                    Console.WriteLine("Producer Sent {0}", message);
                }
            }

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();
        }

        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0) ? string.Join(" ", args) : "Hello World!");
        }
    }
}
