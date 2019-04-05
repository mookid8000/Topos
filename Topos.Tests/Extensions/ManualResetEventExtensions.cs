using System;
using System.Threading;

namespace Topos.Tests.Extensions
{
    public static class ManualResetEventExtensions
    {
        public static void WaitOrDie(this ManualResetEvent manualResetEvent, int timeoutSeconds = 10, string errorMessage = null)
        {
            if (manualResetEvent.WaitOne(TimeSpan.FromSeconds(timeoutSeconds))) return;

            throw new TimeoutException($"The reset event was not set within {timeoutSeconds} s timeout - {errorMessage ?? "no additional details were included"}");
        }
    }
}