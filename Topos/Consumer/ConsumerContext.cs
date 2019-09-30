using System;
using System.Collections.Concurrent;

namespace Topos.Consumer
{
    public class ConsumerContext
    {
        readonly ConcurrentDictionary<string, object> _items = new ConcurrentDictionary<string, object>();

        public void SetItem<T>(string key, T item) where T : class => _items[key] = item;

        public T GetItem<T>(string key) where T : class
        {
            if (!_items.TryGetValue(key, out var result)) return null;

            try
            {
                return (T) result;
            }
            catch (Exception exception)
            {
                throw new ArgumentException($"Item with key '{key}' of type {result.GetType()} could not be turned into {typeof(T)}", exception);
            }
        }
    }
}