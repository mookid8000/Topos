using System;
using NUnit.Framework;
using Testy;
using Topos.Faster.Tests.Factories;
using Topos.Internals;

namespace Topos.Faster.Tests;

[TestFixture]
public class TestAzureBlobsHelper : FixtureBase
{
    [TestCase(BlobStorageDeviceManagerFactory.StorageConnectionString)]
    [TestCase("DefaultEndpointsProtocol=https;AccountName=thisismyaccount;AccountKey=QFV1hggji4oj3g8493j8g9438t9p43u84u89gpu8943pug849ug8439pug843ugp9843g934ug84u8g94u38fQ==;EndpointSuffix=core.windows.net")]
    public void VerifyValidConnectionString(string connectionString)
    {
        Assert.That(AzureBlobsHelper.IsValidConnectionString(connectionString), Is.True, 
            $@"Expected

    {connectionString}

to be considered valid.");
    }

    [Test]
    public void CanCreateContainer()
    {
        var containerName1 = Guid.NewGuid().ToString("n");
        var containerName2 = Guid.NewGuid().ToString("n");

        Using(new StorageContainerDeleter(containerName1));
        Using(new StorageContainerDeleter(containerName2));

        var helper = new AzureBlobsHelper(BlobStorageDeviceManagerFactory.StorageConnectionString);

        Assert.That(helper.CreateContainerIfNotExists(containerName1), Is.True);
        Assert.That(helper.CreateContainerIfNotExists(containerName1), Is.False);

        Assert.That(helper.CreateContainerIfNotExists(containerName2), Is.True);
        Assert.That(helper.CreateContainerIfNotExists(containerName2), Is.False);
        Assert.That(helper.CreateContainerIfNotExists(containerName2), Is.False);
        Assert.That(helper.CreateContainerIfNotExists(containerName2), Is.False);
    }
}