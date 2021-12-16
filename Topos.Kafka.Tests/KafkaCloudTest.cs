﻿using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Testy;
using Testy.Extensions;
using Topos.Config;
using Topos.Logging.Console;
using Topos.Producer;
using Topos.Serialization;
// ReSharper disable ArgumentsStyleAnonymousFunction
#pragma warning disable 1998
// ReSharper disable ArgumentsStyleOther
// ReSharper disable ArgumentsStyleStringLiteral

namespace Topos.Kafka.Tests;

[TestFixture]
public class KafkaCloudTest : FixtureBase
{
    string host;
    string key;
    string secret;

    protected override void SetUp()
    {
        var lines = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "confluent_cloud.secret.txt"));

        host = lines[0];
        key = lines[1];
        secret = lines[2];
    }

    [Test]
    public async Task CanConnectToKafkaCloud()
    {
        const string topic = "test-topic";

        using var producer = Configure
            .Producer(p => p.UseKafka(host).WithConfluentCloud(key, secret))
            .Logging(l => l.UseConsole(minimumLogLevel: LogLevel.Info))
            .Serialization(s => s.UseNewtonsoftJson())
            .Create();

        await producer.Send(topic, new ToposMessage("her er noget nyt!!"));
            
        //await Task.WhenAll(Enumerable.Range(0, 10000).Select(n => 
        //producer.Send(topic, new ToposMessage($"her er besked nr {n}"), partitionKey: (n%20).ToString())));

        var receivedMessages = new ConcurrentQueue<ReceivedLogicalMessage>();

        using var consumer = Configure
            .Consumer("default", c => c.UseKafka(host).WithConfluentCloud(key, secret))
            .Logging(l => l.UseConsole(minimumLogLevel: LogLevel.Info))
            .Serialization(s => s.UseNewtonsoftJson())
            .Topics(t => t.Subscribe(topic))
            .Positions(p => p.StoreInFileSystem(NewTempDirectory()))
            .Handle(async (events, context, token) =>
            {
                foreach (var evt in events)
                {
                    receivedMessages.Enqueue(evt);
                }
            })
            .Create();

        consumer.Start();

        await receivedMessages.WaitOrDie(q => q.Count >= 1, timeoutSeconds: 20);

        await Task.Delay(TimeSpan.FromSeconds(1));

        receivedMessages.Select(s => new { Pos = s.Position, Msg = s.Body?.ToString() }).DumpTable();
    }
}