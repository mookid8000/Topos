using System;
using System.Threading;
using System.Threading.Tasks;

namespace Topos.Extensions
{
    public static class TaskExtensions
    {
        public static bool WaitSafe(this Task task, TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            try
            {
                return task.Wait((int)timeout.TotalMilliseconds, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                return true;
            }
            catch (Exception) when (task.Status == TaskStatus.Canceled)
            {
                return true;
            }
        }

        public static void WaitSafe(this Task task, CancellationToken cancellationToken = default)
        {
            try
            {
                task.Wait(cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // no problem
            }
            catch (Exception) when (task.Status == TaskStatus.Canceled)
            {
                // no problem
            }
        }
    }
}