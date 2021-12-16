using System;
using System.Collections.Concurrent;

namespace Topos.Helpers;

public class Disposables : IDisposable
{
    readonly ConcurrentStack<IDisposable> _disposables = new ConcurrentStack<IDisposable>();

    public void Add(IDisposable disposable) => _disposables.Push(disposable);

    public void Dispose()
    {
        while (_disposables.TryPop(out var disposable))
        {
            disposable.Dispose();
        }
    }
}