using System;
using NUnit.Framework;
using Testy;
using Topos.Faster.Tests.Factories;

namespace Topos.Faster.Tests;

[TestFixture(typeof(FileSystemDeviceManagerFactory))]
[TestFixture(typeof(BlobStorageDeviceManagerFactory))]
public class TestDeviceManager<TDeviceManagerFactory> : FixtureBase where TDeviceManagerFactory : IDeviceManagerFactory, new()
{
    IDeviceManager _deviceManager;

    protected override void SetUp()
    {
        base.SetUp();

        _deviceManager = Using(new TDeviceManagerFactory()).Create();

        if (_deviceManager is IDisposable disposable)
        {
            Using(disposable);
        }
    }

    [Test]
    public void CanCreateTwoLogsForTwoDifferentTopics()
    {
        var cancellationToken = CancelAfter(TimeSpan.FromSeconds(2));

        using var log1 = _deviceManager.GetWriter("topic1", cancellationToken);
        using var log2 = _deviceManager.GetWriter("topic2", cancellationToken);

        Assert.That(log1, Is.Not.EqualTo(log2));
    }

    [Test]
    public void CanCreateTwoLogsForSameTopic()
    {
        var cancellationToken = CancelAfter(TimeSpan.FromSeconds(2));
        
        using var log1_1 = _deviceManager.GetWriter("topic1", cancellationToken);
        using var log1_2 = _deviceManager.GetWriter("topic1", cancellationToken);

        Assert.That(log1_1, Is.EqualTo(log1_2));
    }
}