using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace Topos.Logging.Console
{
    class StringRenderer
    {
        static readonly Regex PlaceholderRegex = new Regex(@"{\w*[\:(\w|\.|\d|\-)*]+}", RegexOptions.Compiled);

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

                        var format = match.Value.Substring(1, match.Value.Length - 2)
                            .Split(':')
                            .Skip(1)
                            .FirstOrDefault();

                        return FormatObject(value, format);
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

        protected virtual string FormatObject(object obj, string format)
        {
            switch (obj)
            {
                case string _:
                    return $@"""{obj}""";

                case IEnumerable enumerable:
                {
                    var valueStrings = enumerable.Cast<object>().Select(o => FormatObject(o, format));

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
}