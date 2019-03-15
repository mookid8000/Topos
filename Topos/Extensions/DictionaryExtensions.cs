using System;
using System.Collections.Generic;

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
    }
}