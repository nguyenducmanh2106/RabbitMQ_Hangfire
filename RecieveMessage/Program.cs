// See https://aka.ms/new-console-template for more information
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

string hostName = "203.162.123.67";
//Here we specify the Rabbit MQ Server. we use rabbitmq docker image and use it
var factory = new ConnectionFactory
{
    HostName = hostName
};
//Create the RabbitMQ connection using connection factory details as i mentioned above
var connection = factory.CreateConnection();
//Here we create channel with session and model
using
var channel = connection.CreateModel();
////declare the queue after mentioning name and a few property related to that
//channel.QueueDeclare("hello", exclusive: false);
////Set Event object which listen message from chanel which is sent by producer
//var consumer = new EventingBasicConsumer(channel);
//consumer.Received += (model, eventArgs) =>
//{
//    var body = eventArgs.Body.ToArray();
//    var message = Encoding.UTF8.GetString(body);
//    Console.WriteLine($"Product message received: {message}");
//};
////read the message
//channel.BasicConsume(queue: "hello", autoAck: true, consumer: consumer);



channel.QueueDeclare(queue: "hello",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: true,
                                     arguments: null);
//channel.QueueDeclare("hello", exclusive: false);

var consumer = new EventingBasicConsumer(channel);
string message = "";
consumer.Received += (model, ea) =>
{
    var body = ea.Body.ToArray();
    message = Encoding.UTF8.GetString(body);
    Console.WriteLine($"Nội dung nhận từ queue : {message}");

};
channel.BasicConsume(queue: "hello",
                     autoAck: true,
                     consumer: consumer);

Console.ReadKey();