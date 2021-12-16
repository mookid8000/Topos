using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Testy;
using Testy.Files;
using Testy.General;
using Topos.Config;
using Topos.Producer;
using Topos.Tests.Contracts.Extensions;

namespace Topos.Faster.Tests;

[TestFixture]
public class CheckMuchData : FixtureBase
{
    [TestCase(100)]
    [TestCase(1_000)]
    [TestCase(10_000)]
    [TestCase(100_000)]
    // works fine - just fills up the disk and then fails with a commit exception, which is totally OK
    //[TestCase(1_000_000)]
    //[TestCase(10_000_000)]
    //[TestCase(100_000_000)]
    //[TestCase(1_000_000_000)]
    public async Task ManyEvents(int count)
    {
        var testDirectory = NewTempDirectory();

        Using(new DisposableCallback(() => PrintDirectoryDetails(testDirectory)));

        using var producer = Configure
            .Producer(p => p.UseFileSystem(testDirectory))
            .Create();

        var messages = Enumerable.Range(0, count)
            .Select(n => new ToposMessage($"THIS IS EVENT NUMBER {n}"));

        foreach (var batch in messages.Batch(1000))
        {
            await producer.SendMany("test", batch);
        }
    }

    [TestCase(1, 1000)]
    [TestCase(1, 2000)]
    [TestCase(1, 3000)]
    [TestCase(1, 4000)]
    [TestCase(1, 4080)]
    [TestCase(1, 4090)]
    [TestCase(1, 4095)]
    [TestCase(1, 4096)]
    [TestCase(1, 4097)]
    [TestCase(1, 5000)]
    [TestCase(1, 8000)]
    [TestCase(1, 8100)]
    [TestCase(1, 8191)]
    // 8192 is exactly too much for pages of size 2^23
    //[TestCase(1, 8192)]
    public async Task BigEvents(int count, int sizeKb)
    {
        var testDirectory = NewTempDirectory();
        var payload = new string('a', count: sizeKb * 1024);

        Using(new DisposableCallback(() => PrintDirectoryDetails(testDirectory)));

        using var producer = Configure
            .Producer(p => p.UseFileSystem(testDirectory))
            .Create();

        var messages = Enumerable.Range(0, count)
            .Select(n => new ToposMessage(payload));

        foreach (var batch in messages.Batch(1000))
        {
            await producer.SendMany("test", batch);
        }
    }

    static void PrintDirectoryDetails(TemporaryTestDirectory testDirectory)
    {
        static decimal ToMb(long bytes) => bytes / (1024m * 1024m);

        var files = new DirectoryInfo(testDirectory).GetFiles("*.*", SearchOption.AllDirectories);
        var sizeMb = ToMb(files.Sum(f => f.Length));

        Console.WriteLine($@"Directory listing:
{string.Join(Environment.NewLine, files.Select(file => $"    {file.Name} ({ToMb(file.Length):0.0} MB)"))}

Total: {sizeMb:0.0} MB");
    }
}