using System;
using NUnit.Framework;
using Topos.Internals;

namespace Topos.Faster.Tests
{
    [TestFixture]
    public class TestSingletonPool
    {
        [Test]
        public void GetsSameInstanceWhenPassingTheSameKey()
        {
            var first = SingletonPool.GetInstance("key", () => new ImportantThing());
            var second = SingletonPool.GetInstance("key", () => new ImportantThing());

            Assert.That(ReferenceEquals(first.Instance, second.Instance), Is.True);
        }

        [Test]
        public void GetsDifferentInstancesWhenPassingDifferentKeys()
        {
            var first = SingletonPool.GetInstance("key1", () => new ImportantThing());
            var second = SingletonPool.GetInstance("key2", () => new ImportantThing());

            Assert.That(ReferenceEquals(first.Instance, second.Instance), Is.False);
        }

        [Test]
        public void DisposesWhenLastReferenceIsDisposed()
        {
            var first = SingletonPool.GetInstance("key", () => new ImportantThing());
            var second = SingletonPool.GetInstance("key", () => new ImportantThing());

            Assert.That(first.Instance.IsDisposed, Is.False);

            first.Dispose();

            Assert.That(first.Instance.IsDisposed, Is.False);

            second.Dispose();

            Assert.That(first.Instance.IsDisposed, Is.True);
        }

        class ImportantThing : IDisposable
        {
            public bool IsDisposed { get; private set; }
            
            public void Dispose()
            {
                IsDisposed = true;
            }
        }
    }
}