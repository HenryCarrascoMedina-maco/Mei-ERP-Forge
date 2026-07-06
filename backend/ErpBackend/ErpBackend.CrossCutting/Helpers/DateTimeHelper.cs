namespace ErpBackend.CrossCutting.Helpers;

/// <summary>
/// Date range, timezone conversion and date calculation helpers.
/// </summary>
public static class DateTimeHelper
{
    public static DateTime StartOfDay(DateTime value) => value.Date;

    public static DateTime EndOfDay(DateTime value) => value.Date.AddDays(1).AddTicks(-1);

    public static DateTime StartOfMonth(DateTime value) => new(value.Year, value.Month, 1, 0, 0, 0, value.Kind);

    public static DateTime EndOfMonth(DateTime value)
        => StartOfMonth(value).AddMonths(1).AddTicks(-1);

    /// <summary>Whole-day difference (to - from).</summary>
    public static int DaysBetween(DateTime from, DateTime to)
        => (int)(to.Date - from.Date).TotalDays;

    /// <summary>Inclusive validation that <paramref name="from"/> is not after <paramref name="to"/>.</summary>
    public static bool IsValidRange(DateTime from, DateTime to) => from <= to;

    /// <summary>Converts a UTC time to the given IANA/Windows time zone id; returns input on failure.</summary>
    public static DateTime ConvertFromUtc(DateTime utc, string timeZoneId)
    {
        try
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), tz);
        }
        catch (TimeZoneNotFoundException)
        {
            return utc;
        }
    }

    /// <summary>Enumerates each date in the inclusive range [from, to].</summary>
    public static IEnumerable<DateTime> EachDay(DateTime from, DateTime to)
    {
        for (var day = from.Date; day <= to.Date; day = day.AddDays(1))
        {
            yield return day;
        }
    }
}
