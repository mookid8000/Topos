using System;
using System.Collections.Concurrent;

namespace Topos.Consumer
{
    public class ConsumerContext
    {
        internal const string ConsumerContextInitializersKey = "consumer-context-initializer-list";

        readonly ConcurrentDictionary<string, object> _items = new ConcurrentDictionary<string, object>();

        public T GetOrAdd<T>(string key, Func<T> factory) => (T)_items.GetOrAdd(key, _ => factory());

        public void SetItem<T>(T item) where T : class => SetItem(GetTypeKey<T>(), item);

        public void SetItem<T>(string key, T item) where T : class => _items[key] = item;

        public T GetItem<T>() where T : class => GetItem<T>(GetTypeKey<T>());

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

        static string GetTypeKey<T>() where T : class => typeof(T).FullName;
    }
}