using RabbitMQ.Client;
using System;
using System.Linq;
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
                    channel.ExchangeDeclare(exchange: "direct_logs",
                                            type: ExchangeType.Direct); // Exchange will only be created if it doesn't exist already.

                    string severity = (args.Length > 0) ? args[0] : "info";

                    string message = GetMessage(args);

                    var body = Encoding.UTF8.GetBytes(message);

                    channel.BasicPublish(exchange: "direct_logs", 
                                         routingKey: severity, 
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
            return ((args.Length > 1) ? string.Join(" ", args.Skip(1).ToArray()) : "Hello World!");
        }
    }
}
