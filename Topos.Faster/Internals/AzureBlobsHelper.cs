using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Storage.Blobs;

namespace Topos.Internals;

class AzureBlobsHelper
{
    readonly string _connectionString;

    public AzureBlobsHelper(string connectionString)
    {
        if (!IsValidConnectionString(connectionString))
        {
            throw new ArgumentException($"Not a valid storage account connection string: '{connectionString}'");
        }

        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public BlobContainerClient GetBlobContainerClient(string containerName) => new(_connectionString, containerName);

    public bool CreateContainerIfNotExists(string containerName)
    {
        var client = GetBlobContainerClient(containerName);
        var response = client.CreateIfNotExists();
        return response != null;
    }

    public static bool IsValidConnectionString(string connectionString)
    {
        if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));

        try
        {
            var keyValuePairs = connectionString.Split(';')
                .Select(part => part.Trim())
                .Select(part => part.Split('=').Select(token => token.Trim()).ToList())
                .Select(tokens => new KeyValuePair<string, string>(tokens.First(), string.Join("=", tokens.Skip(1))))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var isDevelopmentStorageConnectionString = keyValuePairs.TryGetValue("UseDevelopmentStorage", out var value) && string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
            var isNormalConnectionString = keyValuePairs.ContainsKey("AccountName") && keyValuePairs.ContainsKey("AccountKey");

            return isDevelopmentStorageConnectionString || isNormalConnectionString;
        }
        catch
        {
            return false;
        }
    }
}