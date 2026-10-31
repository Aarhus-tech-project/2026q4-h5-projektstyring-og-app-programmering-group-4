namespace Elwise.Core.Time;

/// <summary>
/// Converts between UTC and Danish time (CET/CEST).
/// </summary>
public static class DanishTime
{
    public static readonly TimeZoneInfo Zone =
        TimeZoneInfo.FindSystemTimeZoneById("Europe/Copenhagen");

    public static DateTimeOffset FromUtc(DateTimeOffset time) =>
        TimeZoneInfo.ConvertTime(time, Zone);
}