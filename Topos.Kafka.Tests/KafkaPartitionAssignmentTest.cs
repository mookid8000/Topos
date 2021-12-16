using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using NUnit.Framework;
using Topos.Config;
using Topos.InMem;
using Topos.Producer;

#pragma warning disable 1998
#pragma warning disable 4014

// ReSharper disable ArgumentsStyleLiteral

namespace Topos.Kafka.Tests
{
    [TestFixture]
    public class KafkaPartitionAssignmentTest : KafkaFixtureBase
    {
        [Test]
        public async Task TryDoingIt()
        {
            var numberOfMessages = 60;

            var positionsStorage = new InMemPositionsStorage();
            var topic = GetNewTopic(numberOfPartitions: 4);

            var locks = new InMemExclusiveLockBandit();
            var stopEverything = Using(new CancellationTokenSource());
            var stopSecondConsumer = Using(new CancellationTokenSource());
            var cancellationToken = stopEverything.Token;
            cancellationToken.Register(() => stopSecondConsumer.Cancel());
            var receivedEvents = new ConcurrentQueue<MyEvent>();

            // start producer that produces an event every second
            var producer = StartProducer(cancellationToken, topic, numberOfMessages);

            // when producer has finished producing, give everyone 10 more seconds to complete what they're doing
            producer.ContinueWith(_ => stopEverything.CancelAfter(TimeSpan.FromSeconds(10)), cancellationToken);

            // wait short while, start consumer
            await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
            var consumer1 = StartConsumer(cancellationToken, "CONSUMER 1", topic, positionsStorage, receivedEvents, locks);

            // wait a while and start another consumer
            await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            var consumer2 = StartConsumer(stopSecondConsumer.Token, "CONSUMER 2", topic, positionsStorage, receivedEvents, locks);

            // stop the second consumer after 20 seconds, so the first goes back to being the only one there
            stopSecondConsumer.CancelAfter(TimeSpan.FromSeconds(20));

            // let things run, but ensure that we die if things don't stop by themselves within 1 minute
            stopEverything.CancelAfter(TimeSpan.FromMinutes(1));

            // wait for all tasks
            await Task.WhenAll(producer, consumer1, consumer2);

            // check stuff
            Assert.That(receivedEvents.Count, Is.GreaterThanOrEqualTo(numberOfMessages), $@"Got the following messages:

{string.Join(Environment.NewLine, receivedEvents.OrderBy(e => e.Number).Select(e => $"    {e.Number}: {e.Text}"))}

");

            Assert.That(receivedEvents.Select(e => e.Text).Distinct().Count(), Is.EqualTo(numberOfMessages), $@"Got the following messages:

{string.Join(Environment.NewLine, receivedEvents.OrderBy(e => e.Number).Select(e => $"    {e.Number}: {e.Text}"))}

");
        }

        static Task StartProducer(CancellationToken cancellationToken, string topic, int numberOfMessages)
        {
            var delayBetweenEachMessage = TimeSpan.FromSeconds(1);

            return StartTask(cancellationToken, "PRODUCER", async () =>
             {
                 var producer = Configure.Producer(c => c.UseKafka(KafkaTestConfig.Address))
                     .Serialization(s => s.UseNewtonsoftJson())
                     .Create();

                 using (producer)
                 {
                     for (var counter = 0; counter < numberOfMessages; counter++)
                     {
                         cancellationToken.ThrowIfCancellationRequested();

                         var myEvent = new MyEvent($"THIS IS EVENT NUMBER {counter}", counter);
                         var partitionKey = Guid.NewGuid().ToString();

                         await producer.Send(topic, new ToposMessage(myEvent), partitionKey);

                         await Task.Delay(delayBetweenEachMessage, cancellationToken);
                     }
                 }
             });
        }

        static Task StartConsumer(CancellationToken cancellationToken, string name, string topic, InMemPositionsStorage positionsStorage, ConcurrentQueue<MyEvent> receivedEvents, InMemExclusiveLockBandit locks)
        {
            return StartTask(cancellationToken, name, async () =>
            {
                var consumer = Configure.Consumer("default", c =>
                    {
                        c.UseKafka(KafkaTestConfig.Address)
                            .OnPartitionsAssigned(async (context, partitions) =>
                            {
                                var tasks = partitions
                                    .Select(async partition =>
                                    {
                                        var key = GetLockKey(partition);
                                        var grabbedLock = await locks.GrabLock(key);
                                        context.SetItem(key, grabbedLock);
                                        return grabbedLock;
                                    })
                                    .ToList();

                                await Task.WhenAll(tasks);
                            })
                            .OnPartitionsRevoked(async (context, partitions) =>
                            {
                                var keys = partitions.Select(GetLockKey).ToList();

                                foreach (var lockToRelease in keys.Select(context.GetItem<IDisposable>))
                                {
                                    lockToRelease?.Dispose();
                                }
                            })
                            ;
                    })
                    .Topics(t => t.Subscribe(topic))
                    .Serialization(s => s.UseNewtonsoftJson())
                    .Handle(async (messages, _, _) =>
                    {
                        foreach (var message in messages)
                        {
                            if (message.Body is MyEvent myEvent)
                            {
                                Console.WriteLine($"{name} got event {myEvent.Number}");
                                receivedEvents.Enqueue(myEvent);
                            }
                        }
                    })
                    .Positions(s => s.StoreInMemory(positionsStorage))
                    .Start();

                using (consumer)
                {
                    cancellationToken.WaitHandle.WaitOne();
                }
            });
        }

        static string GetLockKey(TopicPartition partition)
        {
            return $"{partition.Topic}/{partition.Partition}";
        }

        static Task StartTask(CancellationToken cancellationToken, string name, Func<Task> task)
        {
            return Task.Run(async () =>
            {
                Console.WriteLine($">BEGIN TASK '{name}'");
                try
                {
                    await task();
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    // we're exiting
                }
                finally
                {
                    Console.WriteLine($"END TASK '{name}'");
                }
            }, cancellationToken);
        }

        class MyEvent
        {
            public string Text { get; }
            public int Number { get; }

            public MyEvent(string text, int number)
            {
                Text = text;
                Number = number;
            }
        }
    }
}