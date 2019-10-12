using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Topos.Consumer;

namespace Topos.Tests.Contracts.Positions
{
    public class PositionsManagerTest<TPositionsManagerFactory> : ToposContractFixtureBase where TPositionsManagerFactory : IPositionsManagerFactory, new()
    {
        TPositionsManagerFactory _factory;

        protected override void AdditionalSetUp() => _factory = new TPositionsManagerFactory();

        protected IPositionManager Create() => _factory.Create();

        [Test]
        public async Task ReturnsDefaultPositionsAtStartup()
        {
            var manager = Create();

            var position1 = await manager.Get(Guid.NewGuid().ToString(), 0);
            var position2 = await manager.Get(Guid.NewGuid().ToString(), 2);
            var position3 = await manager.Get(Guid.NewGuid().ToString(), 32);
            var position4 = await manager.Get(Guid.NewGuid().ToString(), 123);

            Assert.That(position1, Is.EqualTo(default(Position?)));
            Assert.That(position2, Is.EqualTo(default(Position?)));
            Assert.That(position3, Is.EqualTo(default(Position?)));
            Assert.That(position4, Is.EqualTo(default(Position?)));
        }

        [Test]
        public async Task CanRoundtripPosition()
        {
            var manager = Create();
            var topic = Guid.NewGuid().ToString("N");
            var originalPosition = new Position(topic, 23, 50);
            await manager.Set(originalPosition);

            var roundtrippedPosition = await manager.Get(topic, 23);

            Assert.That(roundtrippedPosition, Is.EqualTo(originalPosition));
        }
    }
}