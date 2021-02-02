using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using NUnit.Framework;
using Topos.Config;
using Topos.Internals;
using Topos.Logging.Console;
using Topos.Producer;
// ReSharper disable ArgumentsStyleStringLiteral
// ReSharper disable AccessToDisposedClosure
#pragma warning disable 1998

namespace Topos.Kafka.Tests
{
    [TestFixture]
    public class TestRawKafkaConsumer : KafkaFixtureBase
    {
        IToposProducer _producer;
        string _topic;
        ConsumerBuilder<byte[], byte[]> _consumerBuilder;

        protected override void SetUp()
        {
            base.SetUp();

            _producer = Configure.Producer(c => c.UseKafka(KafkaTestConfig.Address)).Create();

            Using(_producer);

            _topic = GetNewTopic();

            var config = new ConsumerConfig
            {
                BootstrapServers = KafkaTestConfig.Address,
                GroupId = "default",
                EnableAutoCommit = false,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            _consumerBuilder = new ConsumerBuilder<byte[], byte[]>(config)
                .SetPartitionsAssignedHandler((_, topicPartitions) =>
                {
                    Console.WriteLine($"Partitions assigned: {string.Join(", ", topicPartitions)}");

                    return topicPartitions
                        .Select(p => p.WithOffset(Offset.Beginning));
                })
                .SetPartitionsRevokedHandler((_, topicPartitionOffsets) =>
                {
                    Console.WriteLine($"Partitions revoked: {string.Join(", ", topicPartitionOffsets)}");

                    return topicPartitionOffsets;
                });
        }

        [Test]
        public async Task ThisIsHowWeParty()
        {
            await _producer.SendMany(_topic, Enumerable.Range(0, 10).Select(n => new ToposMessage($"THIS IS MESSAGE {n}")), partitionKey: "whatever");

            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            using var consumer = _consumerBuilder.Build();

            consumer.Subscribe(_topic);

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

        static IEnumerable<TestRun> GetTestRuns() => new[]
        {
            new TestRun(messageCount: 10, useTopos: true, printMessages: true),
            new TestRun(messageCount: 10, useTopos: false, printMessages: true),

            new TestRun(messageCount: 1000, useTopos: true, printMessages: false),
            new TestRun(messageCount: 1000, useTopos: false, printMessages: false),

            new TestRun(messageCount: 100000, useTopos: true, printMessages: false),
            new TestRun(messageCount: 100000, useTopos: false, printMessages: false),

            new TestRun(messageCount: 1000000, useTopos: true, printMessages: false),
            new TestRun(messageCount: 1000000, useTopos: false, printMessages: false),
        };

        public class TestRun
        {
            public int MessageCount { get; }
            public bool UseTopos { get; }
            public bool PrintMessages { get; }

            public TestRun(int messageCount, bool useTopos, bool printMessages)
            {
                MessageCount = messageCount;
                UseTopos = useTopos;
                PrintMessages = printMessages;
            }

            public override string ToString() => $"{MessageCount} msgs with {(UseTopos ? "Topos" : "IConsumer")} (output = {PrintMessages})";
        }

        [TestCaseSource(nameof(GetTestRuns))]
        public async Task ThisIsHowWeParty_TakeTime(TestRun testRun)
        {
            var messageCount = testRun.MessageCount;
            var rawConsumer = !testRun.UseTopos;
            var printMessages = testRun.PrintMessages;

            using (TimerScope("produce", countForRateCalculation: messageCount))
            {
                await _producer.SendMany(_topic, Enumerable.Range(0, messageCount)
                    .Select(n => new ToposMessage($"THIS IS MESSAGE {n}")), partitionKey: "whatever");
            }

            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(2));

            var cancellationToken = cancellationTokenSource.Token;

            using (TimerScope("consume", countForRateCalculation: messageCount))
            {
                if (rawConsumer)
                {
                    using var consumer = _consumerBuilder.Build();

                    consumer.Subscribe(_topic);

                    var receivedMessages = 0;

                    try
                    {
                        while (true)
                        {
                            var result = consumer.Consume(cancellationToken);
                            var message = result.Message;

                            if (printMessages)
                            {
                                var str = Encoding.UTF8.GetString(message.Value);

                                Console.WriteLine(str);
                            }

                            receivedMessages++;

                            if (receivedMessages < messageCount) continue;

                            cancellationTokenSource.Cancel();
                        }
                    }
                    catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
                    {
                    }
                }
                else
                {
                    var receivedMessages = 0;
                    var isCancelled = false;

                    using var consumer = Configure
                        .Consumer(groupName: "default", c => c.UseKafka(KafkaTestConfig.Address))
                        .Logging(l => l.UseConsole(minimumLogLevel: LogLevel.Info))
                        .Topics(t => t.Subscribe(_topic))
                        .Positions(p => p.StoreInMemory())
                        .Handle(async (messages, _, _) =>
                        {
                            receivedMessages += messages.Count;

                            if (printMessages)
                            {
                                foreach (var message in messages)
                                {
                                    Console.WriteLine((string)message.Body);
                                }
                            }

                            if (receivedMessages < messageCount) return;
                            if (isCancelled) return;

                            cancellationTokenSource.Cancel();
                            isCancelled = true;
                        })
                        .Create();

                    cancellationToken.Register(() => consumer.Dispose());

                    consumer.Start();

                    if (!cancellationToken.WaitHandle.WaitOne(timeout: TimeSpan.FromMinutes(1)))
                    {
                        throw new AssertionException(
                            $"{messageCount} messages were not received within 2 minute timeout");
                    }
                }
            }
        }
    }
}