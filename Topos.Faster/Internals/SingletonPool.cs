using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable ArgumentsStyleAnonymousFunction

namespace Topos.Internals;

class SingletonPool
{
    static readonly ConcurrentDictionary<string, PooledObject> _pool = new();

    public static IReadOnlyList<string> Keys => _pool.Keys.ToList();

    public static IReadOnlyList<string> ActiveKeys => _pool.Where(kvp => kvp.Value.ReferenceCount > 0).Select(kvp => kvp.Key).ToList();

    public static Singleton<TInstance> GetInstance<TInstance>(string key, Func<TInstance> factory) where TInstance : IDisposable
    {
        var pooledObject = _pool.AddOrUpdate(
            key: key,
            addValueFactory: _ => PooledObject.New(() => factory()).Increment(),
            updateValueFactory: (_, existing) => existing.Increment()
        );

        var objValue = pooledObject.LazyObject.Value;
        var instance = GetInstanceAs<TInstance>(objValue);

        return new Singleton<TInstance>(instance, () =>
        {
            var result = _pool.AddOrUpdate(
                key: key,
                addValueFactory: _ => PooledObject.New(() => factory()),
                updateValueFactory: (_, existing) => existing.Decrement()
            );

            result.MaybeDispose();
        });
    }

    public class Singleton<TInstance> : IDisposable
    {
        readonly Action _disposeAction;

        public Singleton(TInstance instance, Action disposeAction)
        {
            _disposeAction = disposeAction;
            Instance = instance;
        }

        public TInstance Instance { get; }

        public void Dispose() => _disposeAction();
    }

    static TInstance GetInstanceAs<TInstance>(IDisposable objValue) where TInstance : IDisposable
    {
        try
        {
            return (TInstance)objValue;
        }
        catch (Exception exception)
        {
            throw new ArgumentException($"Could not return instance of type {objValue.GetType()} as {typeof(TInstance)}", exception);
        }
    }

    class PooledObject
    {
        public static PooledObject New(Func<IDisposable> factory) => new(new Lazy<IDisposable>(factory), 0, factory);

        public Lazy<IDisposable> LazyObject { get; private set; }

        public int ReferenceCount { get; }

        public Func<IDisposable> OriginalFactory { get; }

        public PooledObject(Lazy<IDisposable> lazyObject, int referenceCount, Func<IDisposable> originalFactory)
        {
            LazyObject = lazyObject;
            ReferenceCount = referenceCount;
            OriginalFactory = originalFactory;
        }

        public PooledObject Increment() => new(LazyObject, ReferenceCount + 1, OriginalFactory);

        public PooledObject Decrement() => new(LazyObject, ReferenceCount - 1, OriginalFactory);

        public void MaybeDispose()
        {
            if (ReferenceCount != 0) return;
            if (!LazyObject.IsValueCreated) return;

            LazyObject.Value.Dispose();

            // re-init lazy so we can revive this bad boy again if called for
            LazyObject = new Lazy<IDisposable>(() => OriginalFactory());
        }
    }
}