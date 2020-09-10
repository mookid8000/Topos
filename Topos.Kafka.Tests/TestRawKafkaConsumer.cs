using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using NUnit.Framework;
using Topos.Config;
using Topos.Producer;
// ReSharper disable ArgumentsStyleStringLiteral

namespace Topos.Kafka.Tests
{
    [TestFixture]
    public class TestRawKafkaConsumer : KafkaFixtureBase
    {
        [Test]
        public async Task ThisIsHowWeParty()
        {
            using var producer = Configure.Producer(c => c.UseKafka(KafkaTestConfig.Address)).Create();

            var topic = GetNewTopic();

            await producer.SendMany(topic, Enumerable.Range(0, 10).Select(n => new ToposMessage($"THIS IS MESSAGE {n}")), partitionKey: "whatever");

            var config = new ConsumerConfig
            {
                BootstrapServers = KafkaTestConfig.Address,
                GroupId = "default",
                EnableAutoCommit = false,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            var builder = new ConsumerBuilder<byte[], byte[]>(config)
                .SetPartitionsAssignedHandler((_, topicPartitions) =>
                {
                    Console.WriteLine($"Partitions assigned: {string.Join(", ", topicPartitions)}");
                    return Enumerable.Empty<TopicPartitionOffset>();
                })
                .SetPartitionsRevokedHandler((_, topicPartitionOffsets) =>
                {
                    Console.WriteLine($"Partitions revoked: {string.Join(", ", topicPartitionOffsets)}");
                    return Enumerable.Empty<TopicPartitionOffset>();
                });

            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            using var consumer = builder.Build();

            consumer.Subscribe(topic);

            try
            {
                while (true)
                {
                    var result = consumer.Consume(cancellationTokenSource.Token);
                    var message = result.Message;

                    var body = Encoding.UTF8.GetString(message.Value);
                    Console.WriteLine($"Got message: {body}");
                }
            }
            catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
            {

            }
        }
    }
}