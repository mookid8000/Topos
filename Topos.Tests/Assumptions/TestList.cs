using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

namespace Topos.Tests.Assumptions
{
    [TestFixture]
    public class TestList
    {
        [TestCase(1000, true)]
        [TestCase(1000, false)]
        public void WhichIsFasterQuestionMark(int iterations, bool reuseTheList)
        {
            var preallocatedList = new List<string>(10000);

            Func<List<string>> GetFreshList = reuseTheList
                ? () =>
                {
                    preallocatedList.Clear();
                    return preallocatedList;
                }
                : () => new List<string>(10000);

            var stopwatch = Stopwatch.StartNew();

            for (var iteration = 0; iteration < iterations; iteration++)
            {
                for (var count = 0; count < 1000; count++)
                {
                    var list = GetFreshList();

                    for (var index = 0; index < list.Count; index++)
                    {
                        list.Add($"STRING NUMBER {index}");
                    }
                }
            }

            var totalSeconds = stopwatch.Elapsed.TotalSeconds;

            Console.WriteLine($"{iterations}k iterations took {totalSeconds:0.0} s");
        }
    }
}