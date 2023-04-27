using System;
using System.Text;
using System.Threading.Tasks;
using FASTER.core;
using FASTER.devices;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Testy;
using Topos.Faster.Tests.Factories;
using Topos.Internals;
using Topos.Logging.Console;
using LogLevel = Topos.Logging.Console.LogLevel;

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

        using var device = new AzureStorageDevice(
            connectionString: BlobStorageDeviceManagerFactory.StorageConnectionString,
            containerName: _containerName,
            directoryName: "events",
            blobName: "data",
            logger: Logger
        );

        Logger.LogInformation("Creating FASTER log");

        var settings = new FasterLogSettings
        {
            LogDevice = device,
            PageSize = Utility.ParseSize("8 MB")
        };

        using var log = new FasterLog(settings, logger: Logger);

        Logger.LogInformation("Enqueueing string");

        await log.EnqueueAsync(Encoding.UTF8.GetBytes("HEJ MED DIG MIN VEN"));

        Logger.LogInformation("Committing");

        await log.CommitAsync(CancelAfter(TimeSpan.FromSeconds(10)));
    }
}