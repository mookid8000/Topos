using System;
using System.Globalization;

namespace Topos.Extensions;

public static class DateTimeOffsetExtensions
{
    /// <summary>
    /// Serializes this instant with the "O" format, i.e. ISO8601-compliant
    /// </summary>
    public static string ToIso8601DateTimeOffset(this DateTimeOffset dateTimeOffset)
    {
        return dateTimeOffset.ToString("O", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Parses an ISO8601-compliant string into a proper <see cref="DateTimeOffset"/>
    /// </summary>
    public static DateTimeOffset ToDateTimeOffset(this string iso8601String)
    {
        if (!DateTimeOffset.TryParseExact(iso8601String, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result))
        {
            throw new FormatException($"Could not parse '{iso8601String}' as a proper ISO8601-formatted DateTimeOffset!");
        }

        return result;
    }

    public static DateTimeOffset Floor(this DateTimeOffset dateTimeOffset, TimeSpan resolution)
    {
        var resolutionTicks = resolution.Ticks;
        var ticks = dateTimeOffset.Ticks;
        var factor = ticks / resolutionTicks;
        return new DateTimeOffset(factor * resolutionTicks, dateTimeOffset.Offset);
    }
}