using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Topos.Consumer;
using Topos.Kafkaesque;
using Topos.Logging;
// ReSharper disable ArgumentsStyleNamedExpression
// ReSharper disable ArgumentsStyleOther

namespace Topos.Config
{
    public static class KafkaesqueConfigurationExtensions
    {
        public static void UseFileSystem(this StandardConfigurer<IProducerImplementation> configurer, string directoryPath)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));

            CheckDirectoryPath(directoryPath);

            StandardConfigurer.Open(configurer).Register(c => new KafkaesqueFileSystemProducerImplementation(directoryPath, c.Get<ILoggerFactory>()));
        }

        public static void UseFileSystem(this StandardConfigurer<IConsumerImplementation> configurer, string directoryPath)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));

            CheckDirectoryPath(directoryPath);

            StandardConfigurer.Open(configurer).Register(c =>
            {
                var loggerFactory = c.Get<ILoggerFactory>();
                var topics = c.Has<Topics>() ? c.Get<Topics>() : new Topics();
                var group = c.Get<GroupId>();
                
                return new KafkaesqueFileSystemConsumerImplementation(
                    directoryPath: directoryPath,
                    loggerFactory: loggerFactory,
                    topics: topics,
                    group.Id,
                    consumerDispatcher: c.Get<IConsumerDispatcher>(),
                    positionManager: c.Get<IPositionManager>()
                );
            });
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