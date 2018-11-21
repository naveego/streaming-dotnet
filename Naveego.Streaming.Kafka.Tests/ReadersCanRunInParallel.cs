using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using Xunit;
using Naveego.Streaming.Kafka.Tests.Models;

namespace Naveego.Streaming.Kafka.Tests
{
    public class ReadersCanRunInParallel
    {
        private int WrittenCount { get; set; }
        private int ProcessedCount { get; set; }
        
        [Theory]
        [InlineData("topic1", 2, 50, 10)]
        [InlineData("topic2", 4, 100, 30)]
        [InlineData("topic3", 16, 500, 30)]
        public async Task CanReadFromTopicInParallel(
            string topic,
            int readerCount,
            int messageCount,
            int timeoutInSeconds)
        {
            ProcessedCount = 0;
            WrittenCount = 0;
            
            var broker = "kafka:9092";
            var groupId = "streaming-tests";
            
            Action[] actions = new Action[readerCount + 1];

            actions[0] =() => WriteMessages(broker, topic, messageCount);
            
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutInSeconds));

            foreach (var i in Enumerable.Range(1, readerCount))
            {
                actions[i] = () => ReadMessages(broker, topic, groupId, cts.Token);
            }

            Parallel.Invoke(actions);
            
            while(ProcessedCount < messageCount || cts.IsCancellationRequested)
                await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);
            
            Assert.Equal(ProcessedCount, WrittenCount);
        }


        public async Task WriteMessages(string broker, string topic, int messageCount)
        {
              
            var testMessages = new Faker<Message>()
                .StrictMode(true)
                .RuleFor(m => m.Name, f => f.Name.FindName())
                .RuleFor(m => m.Address, f => f.Address.FullAddress())
                .RuleFor(m => m.Company, f => f.Company.CompanyName())
                .RuleFor(m => m.DistanceInMiles, f => f.Random.Byte())
                .RuleFor(m => m.Time, f => f.Date.Soon());

            var writer = new KafkaStreamWriter<Message>(broker, topic);

            foreach (var _ in Enumerable.Range(0, messageCount))
            {
                var m = testMessages.Generate();
                await writer.WriteAsync(m);
                WrittenCount++;
            }
        }

        public async Task ReadMessages(string broker, string topic, string groupId, CancellationToken token)
        {
            var reader = new KafkaStreamReader<Message>(broker, groupId, topic);
            
            await reader.ReadAsync(SomeAsyncTask, token);
        }

        public async Task<HandleResult> SomeAsyncTask(Message m)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(200));
            ProcessedCount++;
            return HandleResult.Ok;
        }
    }
}