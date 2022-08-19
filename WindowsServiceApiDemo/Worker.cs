using System;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace WorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        public static string ApiCommand { get; set; } = "no command";
        public static string hostName { get; set; } = "203.162.123.67";

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    _logger.LogInformation("Worker running at: {time}, command: {string}", DateTimeOffset.Now, ApiCommand);
            //    RecurringJob.AddOrUpdate(() => CronJob(), Cron.Minutely);

            //    RecurringJob.AddOrUpdate(() => CronJob1(), Cron.MinuteInterval(1));

            //    BackgroundJob.Schedule(() => JobRun(), TimeSpan.FromMinutes(2));

            //    ////Sau bao nhiêu miliseconds sẽ gọi thêm một lần thực hiện nữa
            //    await Task.Delay(3600000, stoppingToken);
            //}
            await Task.Run(() =>
            {
                _logger.LogInformation("Worker running at: {time}, command: {string}", DateTimeOffset.Now, ApiCommand);

                RecurringJob.AddOrUpdate(() => CronJob(), Cron.MinuteInterval(1));
                //RecurringJob.AddOrUpdate(() => Consumer(), Cron.MinuteInterval(2));

                //BackgroundJob.Schedule(() => JobSchedule(), TimeSpan.FromMinutes(2));

            });
        }

        public void JobSchedule()
        {
            _logger.LogInformation($"Job will run after 2 minutes");
            BackgroundJob.Enqueue(() => JobEnqueue());
        }


        public void JobEnqueue()
        {
            _logger.LogInformation($"Job Enqueue runned");
        }

        public void CronJob()
        {
            _logger.LogInformation($"Job run every minute");

            var factory = new ConnectionFactory() { HostName = hostName };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: true,
                                     arguments: null);

                //channel.QueueDeclare("hello", exclusive: false);

                string message = "Latitude: " + 192.11 + ", Longitude: " + 23.11 + " and Time: " + DateTime.Now;
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: "hello",
                                     basicProperties: null,
                                     body: body);

            }

        }

        public void Consumer()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "hello",
                                     durable: false,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                string message = "";
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    message = Encoding.UTF8.GetString(body);
                    _logger.LogInformation("Nội dung nhận từ queue : {0}", message);

                };
                channel.BasicConsume(queue: "hello",
                                     autoAck: true,
                                     consumer: consumer);

            }
        }

    }
}
