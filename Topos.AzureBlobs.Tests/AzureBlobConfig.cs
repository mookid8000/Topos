using System;
using System.IO;
using Microsoft.WindowsAzure.Storage;

namespace Topos.AzureBlobs.Tests
{
    static class AzureBlobConfig
    {
        public static CloudStorageAccount StorageAccount
        {
            get
            {
                var filePath = Path.Combine(AppContext.BaseDirectory, "azure_storage_account_connection_string.secret.txt");

                if (File.Exists(filePath)) return CloudStorageAccount.Parse(File.ReadAllText(filePath));

                const string varName = "azure_storage_account_connection_string";
                var environmentVariable = Environment.GetEnvironmentVariable(varName);

                if (!string.IsNullOrWhiteSpace(environmentVariable))
                {
                    return CloudStorageAccount.Parse(environmentVariable);
                }

                throw new ApplicationException($@"Could not get Azure storage account connection string!

Tried loading it from the file

    {filePath}

but the file does not exist.

Also tried looking at the ENV variable named

    {varName}

but it was empty.

Please provide a connection string through one of these for the tests to run.");
            }
        }
    }
}