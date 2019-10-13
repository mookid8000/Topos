using System;
using System.Collections.Generic;

namespace Topos.Config
{
    public class Options
    {
        readonly Dictionary<string, object> _options = new Dictionary<string, object>();

        public void Set(string key, object value) => _options[key ?? throw new ArgumentNullException(nameof(key))] = value;

        public TValue Get<TValue>(string key, TValue defaultValue)
        {
            if (!_options.TryGetValue(key, out var result))
            {
                return defaultValue;
            }

            try
            {
                return (TValue)Convert.ChangeType(result, typeof(TValue));
            }
            catch (Exception exception)
            {
                throw new ArgumentException($"The options value '{result}' of type {result.GetType()} found under key '{key}' could not be automatically converted to {typeof(TValue)}", exception);
            }
        }
    }
}