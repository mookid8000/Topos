using System;
using System.IO;
using System.Threading.Tasks;
using Topos.Consumer;
#pragma warning disable 1998

namespace Topos.FileSystem;

public class FileSystemPositionsManager : IPositionManager
{
    readonly string _directoryPath;

    public FileSystemPositionsManager(string directoryPath)
    {
        _directoryPath = directoryPath;
        EnsureDirectoryExists(directoryPath);
    }

    static void EnsureDirectoryExists(string directoryPath)
    {
        if (Directory.Exists(directoryPath)) return;

        try
        {
            Directory.CreateDirectory(directoryPath);
        }
        catch
        {
            if (!Directory.Exists(directoryPath))
            {
                throw;
            }
        }
    }

    public async Task Set(Position position)
    {
        var filePath = GetFilePath(position.Topic, position.Partition);

        File.WriteAllText(filePath, position.Offset.ToString());
    }

    public async Task<Position> Get(string topic, int partition)
    {
        var filePath = GetFilePath(topic, partition);

        try
        {
            var text = File.ReadAllText(filePath);

            if (long.TryParse(text, out var resut))
            {
                return new Position(topic, partition, resut);
            }

            throw new FormatException($"The text '{text}' from the positions file '{filePath}' could not be interpreted as a valid offset!");
        }
        catch (FileNotFoundException)
        {
            return Position.Default(topic, partition);
        }
    }

    string GetFilePath(string topic, int partition) => Path.Combine(_directoryPath, $"pos-{topic}.{partition}.txt");
}