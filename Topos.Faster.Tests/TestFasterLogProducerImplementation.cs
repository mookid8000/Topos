using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NUnit.Framework;
using Testy;
using Topos.Internals;
using Topos.Logging.Console;
using Topos.Serialization;
// ReSharper disable ConvertToUsingDeclaration

namespace Topos.Faster.Tests;

[TestFixture]
public class TestFasterLogProducerImplementation : FixtureBase
{
    [Test]
    public async Task CanExitQuickly()
    {
        using var _ = TimerScope("start and stop real quick");

        var stopwatch = Stopwatch.StartNew();

        var loggerFactory = new ConsoleLoggerFactory(LogLevel.Debug);

        using (var deviceManager = new DefaultDeviceManager(loggerFactory, NewTempDirectory()))
        {
            var serializer = new ProtobufLogEntrySerializer();
            
            using var eventExpirationHelper = new EventExpirationHelper(loggerFactory, deviceManager, serializer, Array.Empty<KeyValuePair<string, TimeSpan>>());
            using var producerImplementation = new FasterLogProducerImplementation(loggerFactory, deviceManager, serializer, eventExpirationHelper);
            
            producerImplementation.Initialize();

            var headers = new Dictionary<string, string>();
            await producerImplementation.Send("test", "test",
                new TransportMessage(headers, new byte[] { 1, 2, 3 }));
        }

        Assert.That(stopwatch.Elapsed, Is.LessThan(TimeSpan.FromSeconds(1)));
    }
}