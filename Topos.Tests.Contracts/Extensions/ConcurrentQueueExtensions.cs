using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Topos.Tests.Contracts.Extensions
{
    public static class ConcurrentQueueExtensions
    {
        public static void Enqueue<TItem>(this ConcurrentQueue<TItem> queue, IEnumerable<TItem> items)
        {
            foreach (var item in items)
            {
                queue.Enqueue(item);
            }
        }
    }
}