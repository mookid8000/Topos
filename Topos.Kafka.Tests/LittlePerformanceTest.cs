using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Serilog.Events;
using Topos.Config;
using Topos.Producer;
#pragma warning disable 1998

namespace Topos.Kafka.Tests
{
    [TestFixture]
    public class LittlePerformanceTest : KafkaFixtureBase
    {
        //[TestCase(100, 10)]
        //[TestCase(1000, 10)]
        ////[TestCase(10000, 15)]
        //[TestCase(100000, 20)]
        [TestCase(1000000, 90)]
        public async Task TakeTime(int eventCount, int consumeTimeoutSeconds)
        {
            SetLogLevelTo(LogEventLevel.Information);

            var topic = GetNewTopic();
            var events = Enumerable.Range(0, eventCount).Select(n => $"THIS STRING MESSAGE IS EVENT NUMBER {n}");

            var toposProducer = Configure.Producer(c => c.UseKafka(KafkaTestConfig.Address))
                .Logging(l => l.UseSerilog())
                .Create();

            Using(toposProducer);

            await Produce(toposProducer, events, eventCount, topic);

            var counter = 0L;

            var consumer = Configure.Consumer("default", c => c.UseKafka(KafkaTestConfig.Address))
                .Logging(l => l.UseSerilog())
                .Topics(t => t.Subscribe(topic))
                .Handle(async (messages, context, token) => Interlocked.Add(ref counter, messages.Count))
                .Positions(p => p.StoreInMemory())
                .Create();

            Using(consumer);

            consumer.Start();

            var stopwatch = Stopwatch.StartNew();
            var timeout = TimeSpan.FromSeconds(consumeTimeoutSeconds);

            while (true)
            {
                var receivedCount = Interlocked.Read(ref counter);

                if (receivedCount == eventCount)
                {
                    Console.WriteLine("Done!");
                    break;
                }

                await Task.Delay(100);

                if (stopwatch.Elapsed > timeout)
                {
                    throw new TimeoutException($"The expected {eventCount} events were not received within timeout {timeout} - only {receivedCount} were received");
                }
            }

            var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;

            Console.WriteLine($"Consuming {eventCount} events took {elapsedSeconds:0.0} s - that's {eventCount / elapsedSeconds:0.0} evt/s");
        }

        static async Task Produce(IToposProducer producer, IEnumerable<string> events, int eventCount, string topic)
        {
            var stopwatch = Stopwatch.StartNew();

            await producer.SendMany(topic, events.Select((evt, index) => new ToposMessage(evt)));

            //await Task.WhenAll(events.Select(async (evt, index) => await producer.Send(topic, new ToposMessage(evt), index.ToString())));

            var elapsedSeconds = stopwatch.Elapsed.TotalSeconds;

            Console.WriteLine($"Producing {eventCount} events took {elapsedSeconds:0.0} s - that's {eventCount / elapsedSeconds:0.0} evt/s");
        }
    }
}