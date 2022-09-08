using RabbitMQ.Client;
using System;
using System.Text;

namespace NewTask
{
    internal class NewTask // This program will schedule tasks to our Work Queue.
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "task_queue",

                                         // Tasks can be lost if RabbitMQ server stops. Two things are required to make sure that messages aren't lost: we need to mark both the queue and messages as durable.
                                         // To make sure that the queue will survive a RabbitMQ node restart, we need to declare it as durable:
                                         durable: true,

                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null); // Queue will only be created if it doesn't exist already.

                    string message = GetMessage(args);

                    var body = Encoding.UTF8.GetBytes(message); // The message content is a byte array.

                    // Now we need to mark our messages as persistent:
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;
                    // Note: Marking messages as persistent doesn't fully guarantee that a message won't be lost. Although it tells RabbitMQ to save the message to disk, there is still a short time window when RabbitMQ has accepted a message and hasn't saved it yet. The persistence guarantees aren't strong, but it's more than enough for our simple task queue.

                    channel.BasicPublish(exchange: "", // Publishing the message into default (nameless) exchange.
                                         routingKey: "task_queue",
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
