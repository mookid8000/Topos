using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Testy;
using Testy.Files;
using Topos.Config;
using Topos.Producer;
using Topos.Tests.Contracts.Extensions;
#pragma warning disable 1998

namespace Topos.Faster.Tests
{
    [TestFixture]
    public class SimpleTest : FixtureBase
    {
        [Test]
        public async Task CanProduceSomeEvents_Produce_then_consume()
        {
            using var gotTheEvent = new ManualResetEvent(initialState: false);
            var testDirectory = NewTempDirectory();

            using var producer = CreateProducer(testDirectory);
            await producer.Send("test-topic", new ToposMessage(new SomeMessage()));
            
            using var consumer = StartConsumer(testDirectory, gotTheEvent);

            gotTheEvent.WaitOrDie(errorMessage: "Did not get the expected events callback");
        }

        [Test]
        public async Task CanProduceSomeEvents_Consume_then_produce()
        {
            using var gotTheEvent = new ManualResetEvent(initialState: false);
            var testDirectory = NewTempDirectory();

            using var consumer = StartConsumer(testDirectory, gotTheEvent);

            using var producer = CreateProducer(testDirectory);
            await producer.Send("test-topic", new ToposMessage(new SomeMessage()));

            gotTheEvent.WaitOrDie(errorMessage: "Did not get the expected events callback");
        }

        static IToposProducer CreateProducer(TemporaryTestDirectory temporaryTestDirectory)
        {
            return Configure
                .Producer(p => p.UseFileSystem(temporaryTestDirectory))
                .Serialization(s => s.UseNewtonsoftJson())
                .Create();
        }

        static IDisposable StartConsumer(TemporaryTestDirectory testDirectory, ManualResetEvent gotTheEvent)
        {
            return Configure
                .Consumer("whatever", c => c.UseFileSystem(testDirectory))
                .Serialization(s => s.UseNewtonsoftJson())
                .Topics(t => t.Subscribe("test-topic"))
                .Positions(p => p.StoreInMemory())
                .Handle(async (messages, context, token) => gotTheEvent.Set())
                .Start();
        }

        class SomeMessage { }
    }
}