using System.Threading.Tasks;
using NUnit.Framework;
using Topos.Consumer;

namespace Topos.MongoDb.Tests
{
    [TestFixture]
    public class TestMongoDbPositionsManager : MongoFixtureBase
    {
        MongoDbPositionManager _positionManager;

        protected override void SetUp()
        {
            _positionManager = new MongoDbPositionManager(GetCleanTestDatabase(), "Positions");
        }

        [Test]
        public async Task CanSetPosition()
        {
            await _positionManager.Set(new Position("test-topic", 1, 100));
            await _positionManager.Set(new Position("test-topic", 2, 100));
            await _positionManager.Set(new Position("test-topic", 7, -1));
            await _positionManager.Set(new Position("test-topic", 7, (long)int.MaxValue + 5));
        }

        [Test]
        public async Task GetsNothingWhenAskingForPositionsInitially()
        {
            var position = await _positionManager.Get("test-topic", 1);

            Assert.That(position, Is.Null, $"Did not expect to receive anything - got this: {position}");
        }

        [Test]
        public async Task CanGetSinglePosition()
        {
            await _positionManager.Set(new Position("test-topic", 2, 100));

            var position = await _positionManager.Get("test-topic", 2);

            Assert.That(position, Is.Not.Null);

            Assert.That(position.Partition, Is.EqualTo(2));
            Assert.That(position.Offset, Is.EqualTo(100));
        }

        [Test]
        public async Task CanGetUpdatedSinglePosition()
        {
            await _positionManager.Set(new Position("test-topic", 2, 100));
            await _positionManager.Set(new Position("test-topic", 2, 101));
            await _positionManager.Set(new Position("test-topic", 2, 110));

            var position = await _positionManager.Get("test-topic", 2);

            Assert.That(position, Is.Not.Null);

            Assert.That(position.Partition, Is.EqualTo(2));
            Assert.That(position.Offset, Is.EqualTo(110));
        }
    }
}