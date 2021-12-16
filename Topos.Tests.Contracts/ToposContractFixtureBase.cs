using System;
using System.Collections.Concurrent;
using NUnit.Framework;

namespace Topos.Tests.Contracts
{
    public abstract class ToposContractFixtureBase
    {
        readonly ConcurrentStack<IDisposable> _disposables = new();

        [SetUp]
        public void SetUp()
        {
            AdditionalSetUp();
        }

        [TearDown]
        public void TearDown()
        {
            CleanUpDisposables();
        }

        protected virtual void AdditionalSetUp()
        {
        }

        protected TDisposable Using<TDisposable>(TDisposable disposable) where TDisposable : IDisposable
        {
            _disposables.Push(disposable);

            return disposable;
        }

        protected void CleanUpDisposables()
        {
            while (_disposables.TryPop(out var disposable))
            {
                disposable.Dispose();
            }
        }
    }
}