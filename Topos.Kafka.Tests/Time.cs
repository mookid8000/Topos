using System;
using System.Diagnostics;
using System.Threading.Tasks;

#pragma warning disable 1998

namespace Topos.Kafka.Tests
{
    public static class Time
    {
        public static async Task Action(string label, Func<Task> action, int? count = null)
        {
            Console.WriteLine($"Action '{label}' starting");

            var stopwatch = Stopwatch.StartNew();

            await action();

            var elapsed = stopwatch.Elapsed;

            if (count == null)
            {
                Console.WriteLine($"Action '{label}' took {elapsed.TotalSeconds:0.0} s");
            }
            else
            {
                Console.WriteLine($"Action '{label}' for {count} took {elapsed.TotalSeconds:0.0} s - that's {count/elapsed.TotalSeconds:0.0} /s");
            }
        }

        public static void Action(string label, Action action, int? count = null) => Action(label, async () => action(), count).Wait();
    }
}