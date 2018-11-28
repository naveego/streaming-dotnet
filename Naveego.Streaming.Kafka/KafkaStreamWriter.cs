using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;

namespace Naveego.Streaming.Kafka
{
    public class KafkaStreamWriter<T> : IStreamWriter<T>
    {
        private readonly JsonSerializer _serializer = new JsonSerializer();
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
                var sb = new StringBuilder();

                using (var jw = new JsonTextWriter(new StringWriter(sb)))
                {
                    _serializer.Serialize(jw, record);
                }

                await _producer.ProduceAsync(_outTopic, new Message<Null, string> {Value = sb.ToString()});
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error writing message to stream");
            }
        }
        
        public void Write(T record)
        {
            try
            {
                var sb = new StringBuilder();

                using (var jw = new JsonTextWriter(new StringWriter(sb)))
                {
                    _serializer.Serialize(jw, record);
                }
                
                var m = new Message<Null, string>
                {
                    Value = sb.ToString()
                };
                
                _producer.BeginProduce(_outTopic, m);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error writing message to stream");
            }
        }
    }
}