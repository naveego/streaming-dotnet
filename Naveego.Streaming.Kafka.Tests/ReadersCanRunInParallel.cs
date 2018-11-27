using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static int _writtenCount;
        private static int _processedCount;

        [Theory]
        [InlineData("dotnet-streaming-1", 2, 100, 4)]
        [InlineData("dotnet-streaming-2", 4, 200, 5)]
        [InlineData("dotnet-streaming-3", 16, 500, 5)]
        public async Task CanReadFromTopicsInParallel(
            string topic,
            int readerCount,
            int messageCount,
            int timeoutInSeconds)
        {
            _writtenCount = 0;
            _processedCount = 0;

            var broker = "kafka:9092";
            var groupId = "streaming-tests";

            await WriteMessages(broker, topic, messageCount);

            var cts = new CancellationTokenSource();
            var actions = new Action[readerCount];

            foreach (var i in Enumerable.Range(0, readerCount))
            {
                actions[i] = () => ReadMessages(broker, topic, groupId, cts.Token);
            }

            Parallel.Invoke(actions);

            var timer = Stopwatch.StartNew();

            while (_processedCount < messageCount)
            {
                if (timer.Elapsed.TotalSeconds > timeoutInSeconds)
                    break;
            }
            
            timer.Stop();

            var timeoutErrorMessage = "Only processed " + _processedCount + " out of " + _writtenCount + " within timeout of " + timeoutInSeconds + " seconds";

            Assert.True(timeoutInSeconds > timer.Elapsed.TotalSeconds, timeoutErrorMessage);
            Assert.Equal(messageCount, _writtenCount);
            Assert.Equal(_writtenCount, _processedCount);
            Assert.Equal(messageCount, _processedCount);
        }


        private async Task WriteMessages(string broker, string topic, int messageCount)
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
                writer.Write(m);
                Interlocked.Increment(ref _writtenCount);
            }

            await Task.Run(() =>
            {
                while (true)
                {
                    if (_writtenCount == messageCount)
                        break;
                }
            });
        }

        public async Task ReadMessages(string broker, string topic, string groupId, CancellationToken token)
        {
            var reader = new KafkaStreamReader<Message>(broker, groupId, topic);

            await reader.ReadAsync(SomeAsyncTask, token);
        }

        public async Task<HandleResult> SomeAsyncTask(Message m)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(10));
            Interlocked.Increment(ref _processedCount);
            return HandleResult.Ok;
        }
    }
}