using System;
using System.Diagnostics;
using System.Threading.Tasks;

#pragma warning disable 1998

namespace Topos.Tests.Kafka
{
    public static class Time
    {
        public static async Task Action(string label, Func<Task> action)
        {
            Console.WriteLine($"Action '{label}' starting");

            var stopwatch = Stopwatch.StartNew();

            await action();

            var elapsed = stopwatch.Elapsed;

            Console.WriteLine($"Action '{label}' took {elapsed.TotalSeconds:0.0} s");
        }

        public static void Action(string label, Action action) => Action(label, async () => action()).Wait();
    }
}