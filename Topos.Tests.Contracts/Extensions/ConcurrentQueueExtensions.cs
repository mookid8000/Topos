using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

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

        public static async Task WaitOrDie<T>(this ConcurrentQueue<T> queue,
            Expression<Func<ConcurrentQueue<T>, bool>> completionExpression,
            Expression<Func<ConcurrentQueue<T>, bool>> failExpression = null,
            int timeoutSeconds = 5, Func<string> failureDetailsFunction = null)
        {
            failExpression = failExpression ?? (_ => false);
            var completionPredicate = completionExpression.Compile();
            var failPredicate = failExpression.Compile();
            var cancellationTokenSource = new CancellationTokenSource();

            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

            var stopwatch = Stopwatch.StartNew();
            var cancellationToken = cancellationTokenSource.Token;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    if (failPredicate(queue))
                    {
                        var details = failureDetailsFunction?.Invoke();

                        throw new ApplicationException($@"Waiting for

    {completionExpression}

on queue failed, because the failure expression

    {failExpression}

was satisfied after {stopwatch.Elapsed.TotalSeconds:0.0} s.

Details:

{details ?? "NONE"}");
                    }
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