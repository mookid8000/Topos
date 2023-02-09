using Testy.Files;
using Topos.Internals;
using Topos.Logging.Console;

namespace Topos.Faster.Tests.Factories;

public class FileSystemDeviceManagerFactory : IDeviceManagerFactory
{
    readonly TemporaryTestDirectory _testDirectory = new();

    public IDeviceManager Create() => new FileSystemDeviceManager(new ConsoleLoggerFactory(LogLevel.Debug), _testDirectory);

    public void Dispose() => _testDirectory.Dispose();
}