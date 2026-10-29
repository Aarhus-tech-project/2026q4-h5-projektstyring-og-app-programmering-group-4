namespace Elwise.Core.Scheduling;

/// <summary>
/// The hours of the day, in Danish time, when the dishwasher may run.
/// If FromHour is greater than ToHour, the hours go past midnight (e.g. 22 to 7).
/// </summary>
/// <param name="FromHour">The first allowed hour (included).</param>
/// <param name="ToHour">The hour when the allowed time ends (not included).</param>
public sealed record AllowedHours(int FromHour, int ToHour)
{
    public bool Contains(int hour) =>
        FromHour < ToHour
            ? hour >= FromHour && hour < ToHour
            : hour >= FromHour || hour < ToHour;
}