using System;
using System.Collections.Generic;

namespace Topos.Config;

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

        return ChangeType<TValue>(key, result);
    }

    public TValue GetOrAdd<TValue>(string key, Func<TValue> factory)
    {
        if (_options.TryGetValue(key, out var result))
        {
            return ChangeType<TValue>(key, result);
        }

        var value = factory();

        _options[key] = value;

        return value;
    }

    static TValue ChangeType<TValue>(string key, object result)
    {
        try
        {
            return (TValue) Convert.ChangeType(result, typeof(TValue));
        }
        catch (Exception exception)
        {
            throw new ArgumentException(
                $"The options value '{result}' of type {result.GetType()} found under key '{key}' could not be automatically converted to {typeof(TValue)}",
                exception);
        }
    }
}