using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Topos.Consumer;
using Topos.Tests.Contracts.Factories;

namespace Topos.Tests.Contracts.Positions;

public class PositionsManagerTest<TPositionsManagerFactory> : ToposContractFixtureBase where TPositionsManagerFactory : IPositionsManagerFactory, new()
{
    TPositionsManagerFactory _factory;

    protected override void AdditionalSetUp() => _factory = new TPositionsManagerFactory();

    protected IPositionManager Create() => _factory.Create();

    [Test]
    public async Task ReturnsDefaultPositionsAtStartup()
    {
        var manager = Create();

        var topic1 = Guid.NewGuid().ToString();
        var topic2 = Guid.NewGuid().ToString();
        var topic3 = Guid.NewGuid().ToString();

        var position1 = await manager.GetAsync(topic1, 0);
        var position2 = await manager.GetAsync(topic2, 2);
        var position3 = await manager.GetAsync(topic3, 32);

        Assert.That(position1, Is.EqualTo(Position.Default(topic1, 0)));
        Assert.That(position2, Is.EqualTo(Position.Default(topic2, 2)));
        Assert.That(position3, Is.EqualTo(Position.Default(topic3, 32)));
    }

    [Test]
    public async Task CanRoundtripPosition()
    {
        var manager = Create();
        var topic = Guid.NewGuid().ToString("N");
        var originalPosition = new Position(topic, 23, 50);
        await manager.SetAsync(originalPosition);

        var roundtrippedPosition = await manager.GetAsync(topic, 23);

        Assert.That(roundtrippedPosition, Is.EqualTo(originalPosition));
    }
}