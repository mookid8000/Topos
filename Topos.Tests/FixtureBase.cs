using System;
using System.Collections.Concurrent;
using NUnit.Framework;

namespace Topos.Tests
{
    public abstract class FixtureBase
    {
        readonly ConcurrentStack<IDisposable> _disposables = new ConcurrentStack<IDisposable>();

        protected void CleanUpDisposables()
        {
            while (_disposables.TryPop(out var disposable))
            {
                disposable.Dispose();
            }
        }

        protected TDisposable Using<TDisposable>(TDisposable disposable) where TDisposable : IDisposable
        {
            _disposables.Push(disposable);
            return disposable;
        }

        [SetUp]
        public void InternalSetUp()
        {
            SetUp();
        }

        protected virtual void SetUp()
        {
        }

        [TearDown]
        public void InternalTearDown()
        {
            CleanUpDisposables();
        }
    }
}
