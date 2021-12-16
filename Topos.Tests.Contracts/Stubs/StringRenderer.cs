using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using FastMember;

namespace Topos.Tests.Contracts.Stubs;

class StringRenderer
{
    static readonly Regex PlaceholderRegex = new Regex(@"{@*\w*[\:(\w|\.|\d|\-)*]+}", RegexOptions.Compiled);

    readonly ConcurrentDictionary<string, Func<object, string>> _valueGetters = new ConcurrentDictionary<string, Func<object, string>>();
    readonly ConcurrentDictionary<Type, Func<object, string>> _formatters = new ConcurrentDictionary<Type, Func<object, string>>();

    public string RenderString(string message, object[] objs)
    {
        try
        {
            var index = 0;

            return PlaceholderRegex.Replace(message, match =>
            {
                try
                {
                    var value = objs[index];
                    index++;

                    var getter = _valueGetters.GetOrAdd(match.Value, _ =>
                    {
                        var parts = match.Value.Substring(1, match.Value.Length - 2).Split(':');

                        if (parts.First().StartsWith("@"))
                        {
                            return FormatObject;
                        }

                        var format = parts
                            .Skip(1)
                            .FirstOrDefault();

                        return v => FormatValue(v, format);
                    });

                    return getter(value);
                }
                catch (IndexOutOfRangeException)
                {
                    return "???";
                }
            });
        }
        catch
        {
            return message;
        }
    }

    string FormatObject(object obj)
    {
        var type = obj.GetType();

        var formatter = _formatters.GetOrAdd(type, _ =>
        {
            if (typeof(IEnumerable<>).IsAssignableFrom(type))
            {
                return o =>
                {
                    var valueStrings = ((IEnumerable)o).Cast<object>().Select(FormatObject);

                    return $"[{string.Join(", ", valueStrings)}]";
                };
            }

            var accessor = TypeAccessor.Create(type);
            var members = accessor.GetMembers();

            return o => FormatObject(o, members, accessor);
        });

        return formatter(obj);
    }

    string FormatObject(object obj, MemberSet members, TypeAccessor accessor)
    {
        return $"{{ {string.Join(", ", members.Select(m => $"{m.Name} = {FormatValue(accessor[obj, m.Name], null)}"))} }}";
    }

    protected virtual string FormatValue(object obj, string format)
    {
        switch (obj)
        {
            case string _:
                return $@"""{obj}""";

            case IEnumerable enumerable:
            {
                var valueStrings = enumerable.Cast<object>().Select(o => FormatValue(o, format));

                return $"[{string.Join(", ", valueStrings)}]";
            }

            case DateTime dateTime:
                return dateTime.ToString(format ?? "O");

            case DateTimeOffset dateTimeOffset:
                return dateTimeOffset.ToString(format ?? "O");

            case IFormattable formattable:
                return formattable.ToString(format, CultureInfo.InvariantCulture);

            case IConvertible convertible:
                return convertible.ToString(CultureInfo.InvariantCulture);

            default:
                return obj.ToString();
        }
    }
}