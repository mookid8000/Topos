using System;
using NUnit.Framework;
using Testcontainers.Kafka;
using Testy.Files;
using Testy.General;
using Topos.Helpers;

namespace Topos.Kafka.Tests;

[SetUpFixture]
public class KafkaTestConfig
{
    static readonly Disposables disposables = new();

    static readonly Lazy<KafkaContainer> KafkaContainer = new(() =>
    {
        var temporaryTestDirectory = new TemporaryTestDirectory();

        disposables.Add(temporaryTestDirectory);

        var kafka = new KafkaBuilder().Build();

        kafka.StartAsync().GetAwaiter().GetResult();

        disposables.Add(new DisposableCallback(() => kafka.StopAsync().GetAwaiter().GetResult()));

        return kafka;
    });

    public static string Address => KafkaContainer.Value.GetBootstrapAddress();

    [OneTimeTearDown]
    public void StopContainerAsync() => disposables.Dispose();
}