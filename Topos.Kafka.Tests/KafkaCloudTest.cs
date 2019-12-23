using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Testy;
using Testy.Extensions;
using Topos.Config;
using Topos.Producer;
// ReSharper disable ArgumentsStyleAnonymousFunction
#pragma warning disable 1998

// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleStringLiteral

namespace Topos.Kafka.Tests
{
    [TestFixture]
    public class KafkaCloudTest : FixtureBase
    {
        const string Key = "paste-key-here";
        const string Secret = "paster-secret-here";

        [Test]
        [Explicit]
        public async Task CanConnectToKafkaCloud()
        {
            const string topic = "test-topic";

            using var producer = Configure
                .Producer(p =>
                {
                    p.UseKafka("pkc-lz6r3.northeurope.azure.confluent.cloud:9092")
                        .WithConfluentCloud(Key, Secret);
                })
                .Serialization(s => s.UseNewtonsoftJson())
                .Create();

            await producer.Send(topic, new ToposMessage("her er en tekststreng"));

            var receivedStrings = new ConcurrentQueue<string>();

            using var consumer = Configure
                .Consumer("default", c =>
                {
                    c.UseKafka("pkc-lz6r3.northeurope.azure.confluent.cloud:9092")
                        .WithConfluentCloud(Key, Secret);
                })
                .Serialization(s => s.UseNewtonsoftJson())
                .Topics(t => t.Subscribe(topic))
                .Positions(p => p.StoreInFileSystem(NewTempDirectory()))
                .Handle(async (events, context, token) =>
                {
                    events.DumpJson();
                    foreach (var str in events.Select(e => e.Body).OfType<string>())
                    {
                        receivedStrings.Enqueue(str);
                    }
                })
                .Create();

            consumer.Start();

            await Task.Delay(TimeSpan.FromMinutes(1));
        }
    }
}