using Microsoft.Azure.Storage;

namespace Topos.AzureBlobs.Tests;

static class AzureBlobConfig
{
    public static CloudStorageAccount StorageAccount => CloudStorageAccount.DevelopmentStorageAccount;
}