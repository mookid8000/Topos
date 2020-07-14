using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Testy;

namespace Topos.Tests.Assumptions
{
    [TestFixture]
    public class TestSemaphoreSlim : FixtureBase
    {
        [Test]
        [Description("let's be absolutely clear about this")]
        public async Task ItWorks()
        {
            var semaphoreSlim = Using(new SemaphoreSlim(initialCount: 1, maxCount: 1));

            await semaphoreSlim.WaitAsync();

            Console.WriteLine($"waiting {DateTime.Now}");

            using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(1));

            try
            {
                await semaphoreSlim.WaitAsync(cancellationTokenSource.Token);
            }
            catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
            {
                Console.WriteLine("done waiting");
            }

            Console.WriteLine($"time is {DateTime.Now}");
        }
    }
}