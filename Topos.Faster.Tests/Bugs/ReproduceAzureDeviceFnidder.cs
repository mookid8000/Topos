using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FASTER.core;
using FASTER.devices;
using Microsoft.Extensions.Logging;
using Nito.AsyncEx;
using NUnit.Framework;
using Testy;
using Testy.Extensions;
using Topos.Faster.Tests.Factories;
using Topos.Internals;
using Topos.Logging.Console;
using LogLevel = Topos.Logging.Console.LogLevel;
#pragma warning disable CS1998
#pragma warning disable CS4014

namespace Topos.Faster.Tests.Bugs;

[TestFixture]
public class ReproduceAzureDeviceFnidder : FixtureBase
{
    static readonly MicrosoftLoggerAdapter Logger = new(new ConsoleLoggerFactory.ConsoleLogger(typeof(ReproduceAzureDeviceFnidder), LogLevel.Debug));
    string _containerName;

    protected override void SetUp()
    {
        base.SetUp();

        _containerName = Guid.NewGuid().ToString();

        Logger.LogInformation("Using container named {containerName}", _containerName);

        Using(new StorageContainerDeleter(_containerName));
    }

    [Test]
    public async Task ThisShouldWork()
    {
        Logger.LogInformation("Initializing device");

        var connectionString = BlobStorageDeviceManagerFactory.StorageConnectionString;

        using var device = new AzureStorageDevice(
            connectionString: connectionString,
            containerName: _containerName,
            directoryName: "events",
            blobName: "data",
            logger: Logger
        );

        Logger.LogInformation("Creating FASTER log");

        var deviceFactory = new AzureStorageNamedDeviceFactory(connectionString, logger: Logger);
        var namingScheme = new DefaultCheckpointNamingScheme(baseName: $"{_containerName}/events");

        using var checkpointManager = new DeviceLogCommitCheckpointManager(deviceFactory, namingScheme);

        var settings = new FasterLogSettings
        {
            LogDevice = device,
            LogCommitManager = checkpointManager,
            PageSize = Utility.ParseSize("8 MB")
        };

        using var log = new FasterLog(settings, logger: Logger);

        Logger.LogInformation("Enqueueing string");

        await log.EnqueueAsync(Encoding.UTF8.GetBytes("HEJ MED DIG MIN VEN"));

        Logger.LogInformation("Committing");

        await log.CommitAsync(CancelAfter(TimeSpan.FromSeconds(5)));
    }

    [TestCase(10)]
    public async Task ThisShouldWork_ReadAndWrite(int count)
    {
        var stringsToWrite = Enumerable.Range(0, count).Select(n => $"HEJ MED DIG MIN VEN NUMMER {n}").ToList();

        var readStrings = new ConcurrentQueue<string>();
        var cancellationToken = CancelAfter(TimeSpan.FromSeconds(10));

        var writerDone = new AsyncManualResetEvent();

        _ = Task.Run(async () =>
        {
            //return;
            var writerLog = GetFasterLog(BlobStorageDeviceManagerFactory.StorageConnectionString);

            foreach (var str in stringsToWrite)
            {
                await Task.Delay(millisecondsDelay: 10, cancellationToken);
                await writerLog.EnqueueAsync(Encoding.UTF8.GetBytes(str), cancellationToken);
                await writerLog.CommitAsync(cancellationToken);
            }

            writerDone.Set();
        }, cancellationToken);

        _ = Task.Run(async () =>
        {
            //await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

            while (true)
            {
                var readerLog = GetFasterLog(BlobStorageDeviceManagerFactory.StorageConnectionString);

                var beginAddress = readerLog.BeginAddress;

                using var iterator = readerLog.Scan(beginAddress, long.MaxValue);

                while (iterator.GetNext(out var bytes, out _, out var currentAddress, out var nextAddress))
                {
                    var str = Encoding.UTF8.GetString(bytes);
                    readStrings.Enqueue(str);
                    Console.WriteLine($"{currentAddress}: {str}");
                    beginAddress = nextAddress;
                }

                Console.WriteLine("DELAYING!");
                await Task.Delay(millisecondsDelay: 100, cancellationToken);
            }

        }, cancellationToken);

        await writerDone.WaitAsync(cancellationToken);

        await readStrings.WaitOrDie(q => q.Count == count, failExpression: q => q.Count > count);
    }

    FasterLog GetFasterLog(string connectionString)
    {
        Logger.LogInformation("Initializing device");

        var device = new AzureStorageDevice(
            connectionString: connectionString,
            containerName: _containerName,
            directoryName: "events",
            blobName: "data",
            logger: Logger
        );

        Using(device);

        Logger.LogInformation("Creating FASTER log");

        var deviceFactory = new AzureStorageNamedDeviceFactory(connectionString, logger: Logger);
        var namingScheme = new DefaultCheckpointNamingScheme(baseName: $"{_containerName}/events");

        var checkpointManager = new DeviceLogCommitCheckpointManager(deviceFactory, namingScheme);

        Using(checkpointManager);

        var settings = new FasterLogSettings
        {
            LogDevice = device,
            LogCommitManager = checkpointManager,
            PageSize = Utility.ParseSize("8 MB")
        };

        var log = new FasterLog(settings, logger: Logger);

        Using(log);

        return log;
    }
}