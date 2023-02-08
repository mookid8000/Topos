using System;
using NUnit.Framework;
using Testy;
using Testy.Files;
using Topos.Internals;
using Topos.Logging.Console;

namespace Topos.Faster.Tests;

[TestFixture(typeof(FileSystemDeviceManagerFactory))]
public class TestDeviceManager<TDeviceManagerFactory> : FixtureBase where TDeviceManagerFactory : IDeviceManagerFactory, new()
{
    private IDeviceManager _deviceManager;

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
        using var log1 = _deviceManager.GetLog("topic1");
        using var log2 = _deviceManager.GetLog("topic2");

        Assert.That(log1, Is.Not.EqualTo(log2));
    }

    [Test]
    public void CanCreateTwoLogsForSameTopic()
    {
        using var log1_1 = _deviceManager.GetLog("topic1");
        using var log1_2 = _deviceManager.GetLog("topic1");

        Assert.That(log1_1, Is.EqualTo(log1_2));
    }

}

public class FileSystemDeviceManagerFactory : IDeviceManagerFactory
{
    readonly TemporaryTestDirectory _testDirectory = new();

    public IDeviceManager Create() => new FileSystemDeviceManager(new ConsoleLoggerFactory(LogLevel.Debug), _testDirectory);

    public void Dispose() => _testDirectory.Dispose();
}

public interface IDeviceManagerFactory : IDisposable
{
    IDeviceManager Create();
}
