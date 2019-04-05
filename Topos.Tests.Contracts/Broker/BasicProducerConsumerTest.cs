using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Serilog.Events;
using Topos.Config;
using Topos.Tests.Extensions;

#pragma warning disable 1998

namespace Topos.Tests.Contracts.Broker
{
    public abstract class BasicProducerConsumerTest<TProducerFactory> : ToposFixtureBase where TProducerFactory : IBrokerFactory, new()
    {
        IBrokerFactory _brokerFactory;

        [Test]
        public async Task CanStartProducer()
        {
            var producer = BrokerFactory.ConfigureProducer().Create();

            Using(producer);

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        [Test]
        public async Task CanStartConsumer()
        {
            var consumer = BrokerFactory.ConfigureConsumer("default-group")
                .Handle(async (messages, token) =>
                {
                    Console.WriteLine($"Received {messages.Count} msgs");
                })
                .Positions(p => p.StoreInMemory())
                .Create();

            Using(consumer);

            await Task.Delay(TimeSpan.FromSeconds(1));
        }

        [Test]
        public async Task CanProduceAndConsume()
        {
            SetLogLevelTo(LogEventLevel.Verbose);

            var producer = BrokerFactory.ConfigureProducer()
                .Topics(m => m.Map<string>("string"))
                .Create();

            Using(producer);

            var gotTheString = new ManualResetEvent(false);

            var consumer = BrokerFactory.ConfigureConsumer("default-group")
                .Handle(async (messages, token) =>
                {
                    var bodies = messages.Select(m => m.Body).ToArray();

                    Console.WriteLine($@"Received these message bodies:

{string.Join(Environment.NewLine, bodies)}");

                    if (bodies.Length != 1) throw new ArgumentException($@"Received an unexpecte number of messages: {bodies.Length} - expected 1");

                    var receivedString = bodies.OfType<string>().First();

                    if (receivedString != "HEJ MED DIG MIN VEN") throw new ArgumentException($@"Received unexpected string: {receivedString} - expected 'HEJ MED DIG MIN VEN'");

                    gotTheString.Set();
                })
                .Topics(t => t.Subscribe("string"))
                .Positions(p => p.StoreInMemory())
                .Start();

            Using(consumer);

            await producer.Send("HEJ MED DIG MIN VEN");

            gotTheString.WaitOrDie(errorMessage: "Waited for the text 'HEJ MED DIG MIN VEN' to arrive in the consumer");
        }

        IBrokerFactory BrokerFactory
        {
            get
            {
                if (_brokerFactory != null) return _brokerFactory;
                _brokerFactory = new TProducerFactory();
                Using(_brokerFactory);
                return _brokerFactory;
            }
        }
    }
}