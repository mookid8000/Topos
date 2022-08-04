using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Topos.Consumer;

namespace Topos.AzureBlobs;

public class AzureBlobsPositionManager : IPositionManager
{
    const int HttpStatusNotFound = 404;
    readonly ConcurrentDictionary<string, string> _legalBlobNames = new();
    readonly Lazy<Func<Task<BlobContainerClient>>> _getContainerReference;
    readonly string _containerName;

    public AzureBlobsPositionManager(string connectionString, string containerName)
    {
        if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
        _containerName = containerName?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(containerName));

        _getContainerReference = new Lazy<Func<Task<BlobContainerClient>>>(() => async () =>
        {
            var client = new BlobServiceClient(connectionString);
            var blobContainerClient = client.GetBlobContainerClient(containerName);
            await blobContainerClient.CreateIfNotExistsAsync();
            return blobContainerClient;
        });
    }

    public async Task Set(Position position)
    {
        var topic = position.Topic;
        var partition = position.Partition;
        var blobName = GetBlobName(topic, partition);

        try
        {
            var container = await _getContainerReference.Value();
            var blob = container.GetBlobClient(blobName);

            using var destination = await blob.OpenWriteAsync(overwrite: true);
            using var writer = new StreamWriter(destination, Encoding.UTF8);
            await writer.WriteAsync(position.Offset.ToString());
        }
        catch (Exception exception)
        {
            throw new IOException($"Could not write position {position} to {blobName} in container {_containerName}", exception);
        }
    }

    public async Task<Position> Get(string topic, int partition)
    {
        var blobName = GetBlobName(topic, partition);

        try
        {
            var container = await _getContainerReference.Value();
            var blob = container.GetBlobClient(blobName);

            using var source = await blob.OpenReadAsync();
            using var reader = new StreamReader(source, Encoding.UTF8);
            var text = await reader.ReadToEndAsync();

            if (string.IsNullOrWhiteSpace(text)) return Position.Default(topic, partition);

            if (!long.TryParse(text, out var position))
            {
                throw new FormatException(
                    $@"The position text read from {blobName} in container {_containerName}

{text}

could not be parsed into a long!");
            }

            return new Position(topic, partition, position);
        }
        catch (RequestFailedException exception) when (exception.Status == HttpStatusNotFound)
        {
            return Position.Default(topic, partition);
        }
        catch (Exception exception)
        {
            throw new IOException($"Could not read position from blob named {blobName} in container {_containerName}", exception);
        }
    }

    string GetBlobName(string topic, int partition) => $"{GetLegalBlobName(topic)}_{partition}_position.txt";

    string GetLegalBlobName(string topic) => _legalBlobNames.GetOrAdd(topic, GenerateLegalBlobName);

    static string GenerateLegalBlobName(string candidate)
    {
        var characters = new List<char>();

        foreach (var c in candidate.Select((character, index) => new { Character = character, Index = index }))
        {
            if (char.IsLetterOrDigit(c.Character))
            {
                characters.Add(c.Character);
                continue;
            }

            // three first characters must be letters or digits
            if (c.Index < 3)
            {
                characters.Add('a');
                continue;
            }
            characters.Add('_');
        }

        // must be at least three characters in length
        while (characters.Count < 3)
        {
            characters.Add('a');
        }

        return new string(characters.ToArray());
    }
}