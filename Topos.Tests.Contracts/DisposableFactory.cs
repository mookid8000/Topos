using System;
using System.Collections.Concurrent;

namespace Topos.Tests.Contracts;

public abstract class DisposableFactory : IDisposable
{
    readonly ConcurrentStack<IDisposable> _disposables = new();

    public TDisposable Using<TDisposable>(TDisposable disposable) where TDisposable : IDisposable
    {
        _disposables.Push(disposable);
        return disposable;
    }

    public void Dispose()
    {
        while (_disposables.TryPop(out var disposable))
        {
            disposable.Dispose();
        }
    }
}