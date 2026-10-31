using Elwise.Core.Prices;
using Elwise.Core.Time;

namespace Elwise.Core.Scheduling;

/// <summary>
/// Finds the cheapest time window from electricity prices.
/// </summary>
public static class CheapestWindowFinder
{
    private const int PeakStartHour = 17;
    private const int PeakEndHour = 21;

    /// <summary>
    /// Finds the cheapest allowed window for a given duration.
    /// </summary>
    /// <param name="prices">
    /// The 15-minute electricity prices ordered by start time.
    /// </param>
    /// <param name="duration">
    /// The required duration, in multiples of 15 minutes.
    /// </param>
    /// <param name="allowedHours">
    /// The hours of the day, in Danish time, when the program may run.
    /// </param>
    /// <param name="now">
    /// The current time. The window cannot start before this time.
    /// </param>
    /// <returns>
    /// The cheapest allowed time window with its average spot price,
    /// or <c>null</c> if no window follows the rules.
    /// </returns>
    /// <remarks>
    /// No part of the window may be between 17:00 and 21:00 Danish time.
    /// </remarks>
    public static PriceWindow? FindCheapest(
        IReadOnlyList<PricePoint> prices,
        TimeSpan duration,
        AllowedHours allowedHours,
        DateTimeOffset now)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duration),
                "Duration must be greater than zero.");
        }

        if (duration.Ticks % TimeSpan.FromMinutes(15).Ticks != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duration),
                "Duration must be a multiple of 15 minutes.");
        }

        int intervalCount = (int)(duration.TotalMinutes / 15);

        if (prices.Count < intervalCount)
        {
            throw new ArgumentException(
                message: "Not enough price points for the specified duration.",
                paramName: nameof(prices));
        }

        decimal cheapestAveragePrice = decimal.MaxValue;
        int cheapestStartIndex = -1;

        for (int startIndex = 0; startIndex <= prices.Count - intervalCount; startIndex++)
        {
            if (prices[startIndex].Start < now)
            {
                continue;
            }

            decimal totalPrice = 0m;
            bool isAllowed = true;

            for (int offset = 0; offset < intervalCount; offset++)
            {
                PricePoint quarter = prices[startIndex + offset];

                if (!IsAllowedQuarter(quarter, allowedHours))
                {
                    isAllowed = false;
                    break;
                }

                totalPrice += quarter.PricePerKwh;
            }

            if (!isAllowed)
            {
                continue;
            }

            decimal averagePrice = totalPrice / intervalCount;

            if (averagePrice < cheapestAveragePrice)
            {
                cheapestAveragePrice = averagePrice;
                cheapestStartIndex = startIndex;
            }
        }

        if (cheapestStartIndex == -1)
        {
            return null;
        }

        DateTimeOffset cheapestStart = prices[cheapestStartIndex].Start;
        DateTimeOffset cheapestEnd = cheapestStart.Add(duration);

        return new PriceWindow(
            cheapestStart,
            cheapestEnd,
            cheapestAveragePrice);
    }

    private static bool IsAllowedQuarter(PricePoint quarter, AllowedHours allowedHours)
    {
        int hour = DanishTime.FromUtc(quarter.Start).Hour;
        bool isPeak = hour is >= PeakStartHour and < PeakEndHour;
        
        return !isPeak && allowedHours.Contains(hour);
    }
}