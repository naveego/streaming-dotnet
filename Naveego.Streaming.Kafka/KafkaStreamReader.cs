using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Naveego.Streaming.Kafka
{
    public class KafkaStreamReader<T> : IStreamReader<T>
    {
        
        private readonly JsonSerializer _serializer = new JsonSerializer();
        
        private readonly ConsumerConfig _config;
        private readonly string _topic;

        public ILogger Logger { get; set; } = NullLogger.Instance;
       
        public KafkaStreamReader(string kafkaBrokers, string groupId, string topic)
        {
            _config = new ConsumerConfig
            {
                BootstrapServers = kafkaBrokers,
                GroupId = groupId,
                EnableAutoCommit = false,
                StatisticsIntervalMs = 5000,
                SessionTimeoutMs = 6000,
                AutoOffsetReset = AutoOffsetResetType.Earliest
            };

            _topic = topic;
        }
        
        public async Task ReadAsync(Func<T, Task<HandleResult>> onMessage, CancellationToken cancellationToken)
        {
            
            try
            {
                using (var c = new Consumer<Ignore, string>(_config))
                {
                    Logger.LogDebug($"Starting Kafka Consumer on topic {_topic}");
                    c.Subscribe(_topic);

                    var consuming = true;
                    // The client will automatically recover from non-fatal errors. You typically
                    // don't need to take any action unless an error is marked as fatal.
                    c.OnError += (_, e) => consuming = !e.IsFatal;

                    while (consuming)
                    {
                        await ConsumeAndProcessMessage(onMessage, c, cancellationToken);
                    }

                    // Ensure the consumer leaves the group cleanly and final offsets are committed.
                    c.Close();
                }
            }
            
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Could not create consumer on Kafka stream: {ex.Message}");
                throw;
            }
        }

        private async Task ConsumeAndProcessMessage(
            Func<T, Task<HandleResult>> onMessage,
            Consumer<Ignore, string> c,
            CancellationToken cancellationToken)
        {
            // Check the cancellation token to see if we need to stop
                cancellationToken.ThrowIfCancellationRequested();
                HandleResult r = null;
                try
                {
                    var cr = c.Consume();
                    T item;
                    using (var jsonReader = new JsonTextReader(new StringReader(cr.Value)))
                    {
                        item = _serializer.Deserialize<T>(jsonReader);
                    }

                    var result = await onMessage(item);
                    r = result;
                    
                    // If the result was a success then commit the offsets
                    // TODO: Improve this to commit offsets in background for performance reasons
                    if (result.Success)
                    {
                        c.Commit();
                        // move on
                        return;
                    }
                    
                    Logger.LogWarning("Processing of message was not successful.  Retrying...");

                    await RetryProcessingMessage(item, onMessage, c, cancellationToken, result);
                }
                catch (Exception e)
                {
                    switch (e)
                    {
                        case ConsumeException exception:
                            Logger.LogError(e, $"Consuming kafka message error: {e.Message}");
                            break;
                        case TopicPartitionOffsetException exception:
                            Logger.LogError(e, $"Committing topic offset error: {e.Message}");
                            if(r != null)
                                await RetryCommit(c, cancellationToken, r);
                            break;
                        case KafkaException exception:
                            Logger.LogError(e, $"Error interacting with kafka: {e.Message}");
                            break;
                        default:
                            throw;
                    }
                }
        }

        private async Task RetryProcessingMessage(
            T item,
            Func<T, Task<HandleResult>> onMessage,
            Consumer<Ignore, string> c,
            CancellationToken cancellationToken,
            HandleResult result)
        {
            var retryCount = 0;
            
            // If we have reached this point the initial processing of the 
            // message was not successful.  So we need to use the retry 
            // strategy to try it again.
            while (await result.RetryStrategy.Next(cancellationToken))
            {
                retryCount++;
                Logger.LogDebug($"Retrying message: retry count {retryCount}");
                
                // Run the processing again
                var retryResult = await onMessage(item);
                
                // If we were successful then commit and move on.
                if (retryResult.Success)
                {
                    c.Commit();
                    break;
                }
            }
        }
        
        private async Task RetryCommit(
            Consumer<Ignore, string> c,
            CancellationToken cancellationToken,
            HandleResult result)
        {
            var retryCount = 0;
            
            // If we have reached this point the initial processing of the 
            // message was not successful.  So we need to use the retry 
            // strategy to try it again.
            while (await result.RetryStrategy.Next(cancellationToken))
            {
                retryCount++;
                Logger.LogDebug($"Retrying commit: retry count {retryCount}");

                try
                {
                    c.Commit();
                }
                catch (Exception e)
                {
                    continue;
                }

                break;
            }
        }
    }
}