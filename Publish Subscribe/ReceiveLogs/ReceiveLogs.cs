using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Linq;
using System.Text;

namespace ReceiveLogs
{
    internal class ReceiveLogs
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

                    // Whenever we connect to RabbitMQ Server we need a fresh, empty queue. To do this we could create a queue with a random name, or, even better - let the server choose a random queue name for us and then once we disconnect the consumer, the queue should be automatically deleted.
                    var queueName = channel.QueueDeclare().QueueName; // When we supply no parameters to QueueDeclare() we create a non-durable, exclusive (used by only one connection/consumer and the queue will be deleted when that connection closes or consumer disconnects), autodelete (queue that has had at least one consumer, is deleted when last consumer unsubscribes) queue.
                    channel.QueueBind(queue: queueName,
                                      exchange: "logs",
                                      routingKey: ""); // The routingKey here in queue binding is basically what we called Binding Key.

                    // Note that we declare the exchange here as well. Because we might start the consumer before the publisher, we want to make sure the exchange exists so that we can bind the queue created to this exchange before we try to consume messages from it.
                    // The messages will be lost if no queue is bound to the exchange yet, but that's okay for us; if no consumer is listening yet we can safely discard the message.

                    Console.WriteLine("Waiting for logs.");

                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine("Consumer Received {0}", message);
                    };

                    channel.BasicConsume(queue: queueName,
                                         autoAck: true,
                                         consumer: consumer);

                    Console.WriteLine(" Press [enter] to exit.");
                    Console.ReadLine();
                }
            }
        }
    }
}
