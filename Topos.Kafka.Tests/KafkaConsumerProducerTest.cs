using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Topos.Config;
using Topos.Logging.Console;
using Topos.Producer;
// ReSharper disable AccessToDisposedClosure
// ReSharper disable ArgumentsStyleNamedExpression
#pragma warning disable 1998

namespace Topos.Kafka.Tests
{
    [TestFixture]
    public class KafkaConsumerProducerTest : KafkaFixtureBase
    {
        [Test]
        public async Task WordCountExample()
        {
            var topicForText = GetNewTopic();
            var topicForWords = GetNewTopic();

            using var textProducer = Configure.Producer(c => c.UseKafka(KafkaTestConfig.Address))
                .Logging(l => l.UseConsole(minimumLogLevel: LogLevel.Info))
                .Serialization(s => s.UseNewtonsoftJson())
                .Create();

            using var tokenizerConsumer = Configure.Consumer("tokenizer", c => c.UseKafka(KafkaTestConfig.Address))
                .Logging(l => l.UseConsole(minimumLogLevel: LogLevel.Info))
                .Topics(t => t.Subscribe(topicForText))
                .Positions(p => p.StoreInMemory())
                .Serialization(s => s.UseNewtonsoftJson())
                .Options(o => o.AddContextInitializer(c => c.SetItem(textProducer)))
                .Handle(async (messages, context, _) =>
                {
                    var producer = context.GetItem<IToposProducer>();

                    var words = messages.Select(m => m.Body).OfType<MessageWithText>()
                        .SelectMany(m => m.Text.Split(' '));

                    var toposMessages = words
                        .Select(word => new ToposMessage(new MessageWithSingleWord(word)));

                    await producer.SendMany(topicForWords, toposMessages);
                })
                .Create();

            var wordCounts = new ConcurrentDictionary<string, int>();

            using var wordCounterConsumer = Configure.Consumer("word-counter", c => c.UseKafka(KafkaTestConfig.Address))
                .Logging(l => l.UseConsole(minimumLogLevel: LogLevel.Info))
                .Topics(t => t.Subscribe(topicForWords))
                .Positions(p => p.StoreInMemory())
                .Serialization(s => s.UseNewtonsoftJson())
                .Handle(async (messages, _, _) =>
                {
                    var words = messages.Select(m => m.Body)
                        .OfType<MessageWithSingleWord>().Select(m => m.Word);

                    foreach (var word in words)
                    {
                        wordCounts.AddOrUpdate(word, _ => 1, (_, value) => value + 1);
                    }
                })
                .Create();

            const string textFromGitHub = @"Nuget packages corresponding to all commits to release branches are available from the following nuget package source (Note: this is not a web URL - you should specify it in the nuget package manger): https://ci.appveyor.com/nuget/confluent-kafka-dotnet. The version suffix of these nuget packages matches the appveyor build number. You can see which commit a particular build number corresponds to by looking at the AppVeyor build history";

            tokenizerConsumer.Start();
            wordCounterConsumer.Start();

            await textProducer.SendMany(topicForText, Enumerable.Range(0, 1000).Select(_ => new ToposMessage(new MessageWithText(textFromGitHub))));

            await Task.Delay(TimeSpan.FromSeconds(10));

            Console.WriteLine($@"Got these word counts:

{string.Join(Environment.NewLine, wordCounts.OrderByDescending(kvp => kvp.Value).Select(kvp => $"    {kvp.Value}: '{kvp.Key}'"))}

SUM:
    {wordCounts.Sum(kvp => kvp.Value)}");
        }

        class MessageWithText
        {
            public string Text { get; }

            public MessageWithText(string text)
            {
                Text = text;
            }
        }

        class MessageWithSingleWord
        {
            public string Word { get; }

            public MessageWithSingleWord(string word)
            {
                Word = word;
            }
        }
    }
}