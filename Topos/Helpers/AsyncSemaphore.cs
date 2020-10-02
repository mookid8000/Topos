using System;
using System.Threading;
using System.Threading.Tasks;

namespace Topos.Helpers
{
    public class AsyncSemaphore : IDisposable
    {
        readonly SemaphoreSlim _semaphore;

        public AsyncSemaphore(int initialCount, int maxCount) => _semaphore = new SemaphoreSlim(initialCount: initialCount, maxCount: maxCount);

        public void Dispose() => _semaphore.Dispose();

        public Task DecrementAsync(CancellationToken cancellationToken) => _semaphore.WaitAsync(cancellationToken);

        public void Increment() => _semaphore.Release();
    }
}