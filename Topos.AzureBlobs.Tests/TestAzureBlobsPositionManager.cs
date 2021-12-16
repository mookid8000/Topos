using System.Threading.Tasks;
using NUnit.Framework;
using Topos.Consumer;

namespace Topos.AzureBlobs.Tests;

[TestFixture]
public class TestAzureBlobsPositionManager 
{
    [Test]
    public async Task GetsDefaultWhenNoPositionExists_ContainerDoesNotExist()
    {
        var manager = new AzureBlobsPositionManager(AzureBlobConfig.StorageAccount, "does-not-exist");

        Assert.That(await manager.Get("whatever", 1), Is.EqualTo(Position.Default("whatever", 1)));
    }

    [Test]
    public async Task CanRoundtripPosition()
    {
        var manager = new AzureBlobsPositionManager(AzureBlobConfig.StorageAccount, "positions");

        await manager.Set(new Position("my-topic", 3, 500));

        var position = await manager.Get("my-topic", 3);

        Assert.That(position, Is.Not.Null);
        Assert.That(position.Topic, Is.EqualTo("my-topic"));
        Assert.That(position.Partition, Is.EqualTo(3));
        Assert.That(position.Offset, Is.EqualTo(500));
    }

    [Test]
    public async Task WorksWithTheseParticularNames()
    {
        var manager = new AzureBlobsPositionManager(AzureBlobConfig.StorageAccount, "backuppositions");

        await manager.Set(new Position("topic70", 3, 500));

        var position = await manager.Get("topic70", 3);

        Assert.That(position, Is.Not.Null);
        Assert.That(position.Topic, Is.EqualTo("topic70"));
        Assert.That(position.Partition, Is.EqualTo(3));
        Assert.That(position.Offset, Is.EqualTo(500));
    }
}