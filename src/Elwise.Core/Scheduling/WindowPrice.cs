using Elwise.Core.Prices;

namespace Elwise.Core.Scheduling;

/// <summary>
/// Works out the average price of a time window that starts at a given time.
/// </summary>
public static class WindowPrice
{
    /// <summary>
    /// Gets the window that starts with the 15-minute price that contains <paramref name="time"/>.
    /// </summary>
    /// <returns>
    /// The window with its average price, or <c>null</c> if the prices do not cover the whole window.
    /// </returns>
    public static PriceWindow? StartingAt(
        IReadOnlyList<PricePoint> prices,
        DateTimeOffset time,
        TimeSpan duration)
    {
        int intervalCount = (int)(duration.TotalMinutes / 15);
        int startIndex = -1;

        for (int index = 0; index < prices.Count; index++)
        {
            if (prices[index].Start <= time && time < prices[index].Start.AddMinutes(15))
            {
                startIndex = index;
                break;
            }
        }

        if (intervalCount <= 0 || startIndex == -1 || startIndex + intervalCount > prices.Count)
        {
            return null;
        }

        decimal totalPrice = 0m;

        for (int offset = 0; offset < intervalCount; offset++)
        {
            totalPrice += prices[startIndex + offset].PricePerKwh;
        }

        DateTimeOffset start = prices[startIndex].Start;

        return new PriceWindow(start, start.Add(duration), totalPrice / intervalCount);
    }
}