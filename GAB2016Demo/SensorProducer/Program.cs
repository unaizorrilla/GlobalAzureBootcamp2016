namespace SensorProducer
{
    using Microsoft.ServiceBus.Messaging;
    using Newtonsoft.Json;
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    class Program
    {
        static Random _next = new Random(212323);

        static void Main(string[] args)
        {
            var tokenSource = new CancellationTokenSource();

            var task = new TaskFactory()
                .StartNew(() => Produce(tokenSource.Token));

            Console.WriteLine("Producing data... press any key to stop!");
            Console.ReadLine();

            tokenSource.Cancel();
            task.Wait();
        }

        static void Produce(CancellationToken cancellationToken)
        {
            var hub = EventHubClient.Create("unaigab2016");

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                var sensor = new Sensor()
                {
                    Value = _next.NextDouble(),
                    Name = ((DateTime.UtcNow.Second & 1) == 1) ? "Sensor-A" : "Sensor-B"
                };

                hub.SendAsync(new EventData(
                    UTF8Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(sensor)))).Wait();
            }
        }
    }

    class Sensor
    {
        public string Name { get; set; }

        public double Value { get; set; }
    }
}
