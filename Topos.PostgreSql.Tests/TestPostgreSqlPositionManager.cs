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
        const string connectionString = "Host=localhost;Port=5432;Username=postgres;Password=postgres;Database=postgres";
        CleanDatabase(connectionString);
        _positionManager = new PostgreSqlPositionManager(connectionString, "my_consumer_group");
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
        var position = await _positionManager.Get("test-topic", partition: 1);

        Assert.That(position, Is.EqualTo(Position.Default("test-topic", partition: 1)));
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