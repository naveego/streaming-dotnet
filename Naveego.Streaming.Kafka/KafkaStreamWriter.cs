using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Naveego.Streaming.Kafka
{
    public class KafkaStreamWriter<T> : IStreamWriter<T>
    {
        private readonly Producer<Null, string> _producer;
        private readonly string _outTopic;

        private ILogger Logger { get; set; } = NullLogger.Instance;

        public KafkaStreamWriter(string kafkaBrokers, string outputTopic)
        {
            var config = new ProducerConfig
            {
                BootstrapServers = kafkaBrokers,
                Acks = 1
            };

            _producer = new Producer<Null, string>(config);
            _outTopic = outputTopic;
        }

        public async Task WriteAsync(T record)
        {
            try
            {
                var m = new Message<Null, string>
                {
                    Value = Utf8Json.JsonSerializer.ToJsonString(record)
                };
                await _producer.ProduceAsync(_outTopic, m);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error writing message to stream");
            }
        }
    }
}