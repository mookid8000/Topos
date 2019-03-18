using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Topos.Tests.Extensions
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

        public static async Task WaitOrDie<T>(this ConcurrentQueue<T> queue,
            Expression<Func<ConcurrentQueue<T>, bool>> completionExpression,
            int timeoutSeconds = 5)
        {
            var completionPredicate = completionExpression.Compile();
            var cancellationTokenSource = new CancellationTokenSource();

            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            var cancellationToken = cancellationTokenSource.Token;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (completionPredicate(queue)) return;

                    await Task.Delay(117, cancellationToken);
                }
            }
            catch (OperationCanceledException) when (cancellationTokenSource.IsCancellationRequested)
            {
            }

            throw new TimeoutException($@"Waiting for

    {completionExpression}

on queue did not complete in {timeoutSeconds} s");
        }
    }
}