using System;
using NUnit.Framework;
using Topos.Logging;

namespace Topos.Tests.Logging
{
    [TestFixture]
    public class TestConsoleLogger : FixtureBase
    {
        [Test]
        public void LogStuff()
        {
            var logger = new ConsoleLogger();

            logger.Debug("This is just debugging info");
            logger.Info("Received {count} things over a duration of {elapsed}", 23, TimeSpan.FromSeconds(4.5));
            logger.Warn("Here's a warning");

            try
            {
                throw new AccessViolationException("OH NO!");
            }
            catch (Exception exception)
            {
                logger.Error(exception, "Unhandled error when doing stuff");
            }
        }
    }
}
