using System;
using System.Threading.Tasks;
using Nito.AsyncEx;
using NUnit.Framework;
using Testy;

namespace Topos.Tests.Assumptions;

[TestFixture]
public class TestAsyncSemaphore : FixtureBase
{
    [Test]
    public void CanBeUsedAsQueueGate()
    {
        var semaphore = new AsyncSemaphore(initialCount: 0);

        semaphore.Release();
        semaphore.Release();
        semaphore.Release();

        var cancellationToken = CancelAfter(TimeSpan.FromSeconds(1));

        semaphore.Wait(cancellationToken);
        semaphore.Wait(cancellationToken);
        semaphore.Wait(cancellationToken);

        var exception = Assert.Throws<TaskCanceledException>(() => semaphore.Wait(cancellationToken));

        Console.WriteLine(exception);
    }
}