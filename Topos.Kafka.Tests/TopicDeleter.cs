using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka.Admin;
using Serilog;
using Topos.Serilog;
// ReSharper disable SimplifyLinqExpression

namespace Topos.Kafka.Tests
{
    public class TopicDeleter : IDisposable
    {
        static readonly ILogger Logger = Log.ForContext<TopicDeleter>();
        readonly string _topicName;
        readonly KafkaProducer _producer;

        public TopicDeleter(string topicName)
        {
            _producer = new KafkaProducer(new SerilogLoggerFactory(Logger), KafkaTestConfig.Address);
            _topicName = topicName;
        }

        public void Dispose()
        {
            using (_producer)
            {
                var adminClient = _producer.GetAdminClient();
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));

                if (!metadata.Topics.Select(t => t.Topic).Contains(_topicName)) return;

                Logger.Information("Deleting topic {topic}", _topicName);

                ExceptionDispatchInfo exception = null;
                var done = new ManualResetEvent(false);

                Task.Run(async () =>
                {
                    try
                    {
                        await adminClient
                            .DeleteTopicsAsync(new[] {_topicName}, new DeleteTopicsOptions
                            {
                                OperationTimeout = TimeSpan.FromSeconds(10)
                            });
                    }
                    catch (Exception ex)
                    {
                        exception = ExceptionDispatchInfo.Capture(ex);
                    }
                    finally
                    {
                        done.Set();
                    }
                });

                if (!done.WaitOne(TimeSpan.FromSeconds(20)))
                {
                    throw new TimeoutException($"Timeout after waiting 20 s for topic {_topicName} to be deleted");
                }

                if (exception != null)
                {
                    Console.WriteLine($"Error when deleting topic {_topicName}: {exception.SourceException}");
                }
            }
        }
    }
}
