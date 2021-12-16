using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Topos.Consumer;
using Topos.Faster;
using Topos.Internals;
using Topos.Logging;
// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther
// ReSharper disable UnusedMember.Global

namespace Topos.Config
{
    public static class FasterLogConfigurationExtensions
    {
        /// <summary>
        /// Configures Topos to use Microsoft's FASTER Log and the file system as the event store
        /// </summary>
        public static FasterProducerConfigurationBuilder UseFileSystem(this StandardConfigurer<IProducerImplementation> configurer, string directoryPath)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));

            var builder = new FasterProducerConfigurationBuilder();

            CheckDirectoryPath(directoryPath);

            StandardConfigurer.Open(configurer)
                .Register(c => new FasterLogProducerImplementation(
                    loggerFactory: c.Get<ILoggerFactory>(),
                    deviceManager: c.Get<IDeviceManager>(),
                    logEntrySerializer: c.Get<ILogEntrySerializer>(),
                    eventExpirationHelper: c.Get<EventExpirationHelper>()
                ))
                .Other<IDeviceManager>().Register(c => new DefaultDeviceManager(
                    loggerFactory: c.Get<ILoggerFactory>(),
                    directoryPath: directoryPath
                ))
                .Other<ILogEntrySerializer>().Register(_ => new ProtobufLogEntrySerializer())
                .Other<EventExpirationHelper>().Register(c => new EventExpirationHelper(
                    loggerFactory: c.Get<ILoggerFactory>(),
                    deviceManager: c.Get<IDeviceManager>(),
                    maxAgesPerTopic: builder.GetMaxAges(),
                    logEntrySerializer: c.Get<ILogEntrySerializer>()
                ));

            return builder;
        }

        /// <summary>
        /// Configures Topos to use Microsoft's FASTER Log and the file system as the event store
        /// </summary>
        public static void UseFileSystem(this StandardConfigurer<IConsumerImplementation> configurer, string directoryPath)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));

            CheckDirectoryPath(directoryPath);

            StandardConfigurer.Open(configurer)
                .Register(c => new FasterLogConsumerImplementation(
                    loggerFactory: c.Get<ILoggerFactory>(),
                    deviceManager: c.Get<IDeviceManager>(),
                    logEntrySerializer: c.Get<ILogEntrySerializer>(),
                    topics: c.Has<Topics>() ? c.Get<Topics>() : new Topics(),
                    group: c.Get<GroupId>().Id,
                    consumerDispatcher: c.Get<IConsumerDispatcher>(),
                    positionManager: c.Get<IPositionManager>()
                ))
                .Other<IDeviceManager>().Register(c => new DefaultDeviceManager(
                    loggerFactory: c.Get<ILoggerFactory>(),
                    directoryPath: directoryPath
                ))
                .Other<ILogEntrySerializer>().Register(_ => new ProtobufLogEntrySerializer());
        }

        static void CheckDirectoryPath(string directoryPath)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
            {
                throw new ArgumentException($"Please remember to set the {nameof(directoryPath)} variable to the path where the file system-based broker's log files will be stored");
            }

            EnsureDirectoryExists();

            VerifyWritability();

            void EnsureDirectoryExists()
            {
                if (Directory.Exists(directoryPath)) return;

                try
                {
                    Directory.CreateDirectory(directoryPath);
                }
                catch (IOException)
                {
                    if (!Directory.Exists(directoryPath)) throw;
                }
            }

            void VerifyWritability()
            {
                var caughtExceptions = new List<Exception>();

                const int maxAttempts = 10;
                for (var counter = 0; counter < maxAttempts; counter++)
                {
                    var testFilePath = Path.Combine(directoryPath, ".topos-test-file");

                    try
                    {
                        const string text =
                            @"This is a file written by Topos to verify that it has the necessary access rights";

                        File.WriteAllText(testFilePath, text);

                        var roundtrippedText = File.ReadAllText(testFilePath);

                        File.Delete(testFilePath);

                        if (roundtrippedText != text)
                        {
                            throw new IOException($@"Tried to roundtrip this text:

{text}

but the text read back from the file {testFilePath} was:

{roundtrippedText}");
                        }

                        return;
                    }
                    catch (Exception exception)
                    {
                        caughtExceptions.Add(exception);
                        Thread.Sleep(TimeSpan.FromSeconds(0.2));
                    }
                }

                throw new AggregateException($"Could not verify write access to directory {directoryPath} after {maxAttempts} attempts", caughtExceptions);
            }
        }
    }
}