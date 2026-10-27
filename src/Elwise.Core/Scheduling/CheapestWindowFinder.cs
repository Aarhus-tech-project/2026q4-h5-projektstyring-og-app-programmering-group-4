using Elwise.Core.Prices;

namespace Elwise.Core.Scheduling;

/// <summary>
/// Finds the cheapest time window from electricity prices.
/// </summary>
public static class CheapestWindowFinder
{
    /// <summary>
    /// Finds the cheapest window for a given duration.
    /// </summary>
    /// <param name="prices">
    /// The 15-minute electricity prices ordered by start time.
    /// </param>
    /// <param name="duration">
    /// The required duration, in multiples of 15 minutes.
    /// </param>
    /// <returns>
    /// The cheapest time window with its average spot price.
    /// </returns>
    public static PriceWindow FindCheapest(
        IReadOnlyList<PricePoint> prices,
        TimeSpan duration)
    {
        if(duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duration),
                "Duration must be greater than zero.");
        }

        if (duration <= TimeSpan.Zero || duration.Ticks % TimeSpan.FromMinutes(15).Ticks != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(duration),
                "Duration must be a multiple of 15 minutes.");
        }

        int intervalCount = (int)(duration.TotalMinutes / 15);

        if (prices.Count < intervalCount)
        {
            throw new ArgumentException(
                nameof(prices),
                "Not enough price points for the specified duration.");
        }

        decimal cheapestAveragePrice = decimal.MaxValue;
        int cheapestStartIndex = -1;
        for (int startIndex = 0; startIndex <= prices.Count - intervalCount; startIndex++)
        {
            decimal totalPrice = 0m;

            for (int offset = 0; offset < intervalCount; offset++)
            {
                totalPrice += prices[startIndex + offset].PricePerKwh;
            }

            decimal averagePrice = totalPrice / intervalCount;

            if (averagePrice < cheapestAveragePrice)
            {
                cheapestAveragePrice = averagePrice;
                cheapestStartIndex = startIndex;
            }
        }
        DateTimeOffset cheapestStart = prices[cheapestStartIndex].Start;
        DateTimeOffset cheapestEnd = cheapestStart.Add(duration);
        return new PriceWindow(
            cheapestStart,
            cheapestEnd,
            cheapestAveragePrice
        );
    }
}