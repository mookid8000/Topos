using System;
using System.Collections.Concurrent;
using System.Threading;
// ReSharper disable StaticMemberInGenericType
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable UnusedTypeParameter

namespace Topos.Consumer;

public class ConsumerContext
{
    static int _index;

    /// <summary>
    /// (ab)use type system to map types to integers
    /// </summary>
    class Id<T>
    {
        internal static readonly int Index = Interlocked.Increment(ref _index);
    }

    readonly object[] _quickItems = new object[128];

    internal const string ConsumerContextInitializersKey = "consumer-context-initializer-list";

    readonly ConcurrentDictionary<string, object> _items = new();

    public T GetOrAdd<T>(string key, Func<T> factory) => (T)_items.GetOrAdd(key, _ => factory());

    public void SetItem<T>(T item) where T : class => _quickItems[Id<T>.Index] = item;

    public void SetItem<T>(string key, T item) where T : class => _items[key] = item;

    public T GetItem<T>() where T : class => _quickItems[Id<T>.Index] as T;

    public T GetItem<T>(string key) where T : class
    {
        if (!_items.TryGetValue(key, out var result)) return null;

        try
        {
            return (T)result;
        }
        catch (Exception exception)
        {
            throw new ArgumentException($"Item with key '{key}' of type {result.GetType()} could not be turned into {typeof(T)}", exception);
        }
    }
}