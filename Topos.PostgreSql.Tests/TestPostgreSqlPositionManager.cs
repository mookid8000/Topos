using System;
using System.Threading.Tasks;
using NUnit.Framework;
using Topos.Consumer;

namespace Topos.PostgreSql.Tests;

[TestFixture]
public class TestPostgreSqlPositionManager : PostgreSqlFixtureBase
{
    PostgreSqlPositionManager _positionManager;

    protected override void SetUp()
    {
        _positionManager = new PostgreSqlPositionManager(ConnectionString, "my_consumer_group");
    }

    [Test]
    public async Task CanSetPosition()
    {
        var testTopic = $"test-topic-{Guid.NewGuid():N}";

        await _positionManager.Set(new Position(testTopic, 1, 100));
        await _positionManager.Set(new Position(testTopic, 2, 100));
        await _positionManager.Set(new Position(testTopic, 7, -1));
        await _positionManager.Set(new Position(testTopic, 7, (long)int.MaxValue + 5));
    }

    [Test]
    public async Task GetsNothingWhenAskingForPositionsInitially()
    {
        var testTopic = $"test-topic-{Guid.NewGuid():N}";

        var position = await _positionManager.Get(testTopic, partition: 1);

        Assert.That(position, Is.EqualTo(Position.Default(testTopic, partition: 1)));
    }

    [Test]
    public async Task CanGetSinglePosition()
    {
        var testTopic = $"test-topic-{Guid.NewGuid():N}";

        await _positionManager.Set(new Position(testTopic, 2, 100));

        var position = await _positionManager.Get(testTopic, 2);

        Assert.That(position, Is.Not.Null);

        Assert.That(position.Partition, Is.EqualTo(2));
        Assert.That(position.Offset, Is.EqualTo(100));
    }

    [Test]
    public async Task CanGetUpdatedSinglePosition()
    {
        var testTopic = $"test-topic-{Guid.NewGuid():N}";

        await _positionManager.Set(new Position(testTopic, 2, 100));
        await _positionManager.Set(new Position(testTopic, 2, 101));
        await _positionManager.Set(new Position(testTopic, 2, 110));

        var position = await _positionManager.Get(testTopic, 2);

        Assert.That(position, Is.Not.Null);

        Assert.That(position.Partition, Is.EqualTo(2));
        Assert.That(position.Offset, Is.EqualTo(110));
    }
}