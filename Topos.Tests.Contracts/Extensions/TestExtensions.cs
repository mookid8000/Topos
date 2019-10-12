using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Topos.Tests.Contracts.Extensions
{
    public static class TestExtensions
    {
        public static IEnumerable<IReadOnlyCollection<T>> Batch<T>(this IEnumerable<T> items, int batchSize)
        {
            var list = new List<T>(batchSize);

            foreach (var item in items)
            {
                list.Add(item);

                if (list.Count < batchSize) continue;

                yield return list.ToArray();

                list.Clear();
            }

            if (list.Any())
            {
                yield return list.ToArray();
            }
        }

        public static void WaitOrDie(this ManualResetEvent manualResetEvent, int timeoutSeconds = 10, string errorMessage = null)
        {
            if (manualResetEvent.WaitOne(TimeSpan.FromSeconds(timeoutSeconds))) return;

            throw new TimeoutException($"The reset event was not set within {timeoutSeconds} s timeout - {errorMessage ?? "no additional details were included"}");
        }
    }
}