using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace Worker
{
    internal class Worker
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queue: "task_queue",
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null); // Queue will only be created if it doesn't exist already.

                    // Note that we declare the queue here as well. Because we might start the consumer before the publisher, we want to make sure the queue exists before we try to consume messages from it.

                    channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false); // Setting prefetchCount to 1 for Fair Dispatching from RabbitMQ Server.

                    var consumer = new EventingBasicConsumer(channel);

                    // RabbitMQ server will deliver/push us the message from the queue asynchronously, so we provide a callback. And that is what EventingBasicConsumer.Received event handler does.
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine("Consumer Received {0}", message);

                        int dots = message.Split('.').Length - 1;
                        Thread.Sleep(dots * 1000);
                        Console.WriteLine("Processing Done");
                        
                        channel.BasicAck(deliveryTag: ea.DeliveryTag,
                                         multiple: false);
                        // By setting autoAck to false and using this code (manual message acknowledgement) we can be sure that even if you kill a worker using CTRL+C while it was processing a message, nothing will be lost. Soon after the worker dies all unacknowledged messages will be redelivered.
                        // Note: Acknowledgement must be sent on the same channel that received the delivery. Attempts to acknowledge using a different channel will result in a channel-level protocol exception.
                    };

                    channel.BasicConsume(queue: "task_queue",
                                         autoAck: false,
                                         consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
        }
    }
}
