using System;
using System.Collections.Generic;
using System.Linq;

namespace Topos.Extensions
{
    public static class DictionaryExtensions
    {
        public static Dictionary<string, string> Clone(this Dictionary<string, string> dictionary)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            return new Dictionary<string, string>(dictionary);
        }

        public static TValue GetValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue defaultValue)
        {
            if (dictionary == null) throw new ArgumentNullException(nameof(dictionary));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (defaultValue == null) throw new ArgumentNullException(nameof(defaultValue));
            return dictionary.TryGetValue(key, out var value)
                ? value
                : defaultValue;
        }

        public static string GetValue(this Dictionary<string, string> headers, string key)
        {
            if (headers == null) throw new ArgumentNullException(nameof(headers));
            if (key == null) throw new ArgumentNullException(nameof(key));

            return headers.TryGetValue(key, out var value)
                ? value
                : throw new KeyNotFoundException($"Could not find '{key}' header among these keys: {string.Join(", ", headers.Keys.Select(k => $"'{k}'"))}");
        }
    }
}