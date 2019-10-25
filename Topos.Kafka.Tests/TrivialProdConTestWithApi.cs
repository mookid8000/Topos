using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Testy.Extensions;
using Topos.Config;
using Topos.Producer;
using Topos.Tests.Contracts.Extensions;

#pragma warning disable 1998

namespace Topos.Kafka.Tests
{
    [TestFixture]
    public class TrivialProdConTestWithApi : KafkaFixtureBase
    {
        string _topic;

        protected override void SetUp()
        {
            _topic = GetNewTopic();
        }

        [Test]
        public async Task ItWorks()
        {
            var receivedEvents = new ConcurrentQueue<string>();

            var producer = Configure
                .Producer(e => e.UseKafka(KafkaTestConfig.Address))
                .Logging(l => l.UseSerilog())
                .Topics(c => c.Map<string>(_topic))
                .Create();

            Using(producer);

            var consumer = Configure
                .Consumer("default-group", e => e.UseKafka(KafkaTestConfig.Address))
                .Logging(l => l.UseSerilog())
                .Topics(t => t.Subscribe(_topic))
                .Positions(p => p.StoreInMemory())
                .Handle(async (messages, context, token) =>
                {
                    foreach (var message in messages.Select(m => m.Body).OfType<string>())
                    {
                        receivedEvents.Enqueue(message);
                    }
                })
                .Start();

            Using(consumer);

            await producer.Send(new ToposMessage("hej med dig min ven!"));

            await receivedEvents.WaitOrDie(c => c.Count >= 1, timeoutSeconds: 10);

            Console.WriteLine($@"Got these events:

{string.Join(Environment.NewLine, receivedEvents)}");
        }
    }
}