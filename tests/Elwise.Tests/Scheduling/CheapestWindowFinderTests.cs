using Elwise.Core.Prices;
using Elwise.Core.Scheduling;
using Elwise.Core.Time;
using Xunit;

namespace Elwise.Tests.Scheduling;

public class CheapestWindowFinderTests
{
    private static readonly AllowedHours AllDay = new(0, 24);

    [Fact]
    public void FindCheapest_ReturnsCheapestTwoHourWindow()
    {
        // Arrange
        var start = new DateTimeOffset(2026, 10, 26, 8, 0, 0, TimeSpan.Zero);

        var prices = new List<PricePoint>
        {
            new PricePoint(start, 1.50m),
            new PricePoint(start.AddMinutes(15), 0.50m),
            new PricePoint(start.AddMinutes(30), 0.50m),
            new PricePoint(start.AddMinutes(45), 0.50m),
            new PricePoint(start.AddMinutes(60), 0.50m),
            new PricePoint(start.AddMinutes(75), 0.50m),
            new PricePoint(start.AddMinutes(90), 0.50m),
            new PricePoint(start.AddMinutes(105), 0.50m),
            new PricePoint(start.AddMinutes(120), 0.50m)
        };

        var duration = TimeSpan.FromHours(2);

        // Act
        var actual = CheapestWindowFinder.FindCheapest(prices, duration, AllDay, now: start);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(start.AddMinutes(15), actual.Start);
        Assert.Equal(start.AddMinutes(135), actual.End);
        Assert.Equal(0.50m, actual.AveragePricePerKwh);
    }

    [Fact]
    public void FindCheapest_CheapestWindowMiddle_ReturnsMiddleWindow()
    {
        // Arrange
        var start = new DateTimeOffset(2026, 10, 26, 8, 0, 0, TimeSpan.Zero);

        var prices = new List<PricePoint>
        {
            new PricePoint(start, 3m),
            new PricePoint(start.AddMinutes(15), 3m),
            new PricePoint(start.AddMinutes(30), 1m),
            new PricePoint(start.AddMinutes(45), 1m),
            new PricePoint(start.AddMinutes(60), 1m),
            new PricePoint(start.AddMinutes(75), 1m),
            new PricePoint(start.AddMinutes(90), 3m),
            new PricePoint(start.AddMinutes(105), 3m)
        };

        var duration = TimeSpan.FromHours(1);

        // Act
        var actual = CheapestWindowFinder.FindCheapest(prices, duration, AllDay, now: start);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(start.AddMinutes(30), actual.Start);
        Assert.Equal(start.AddMinutes(90), actual.End);
        Assert.Equal(1m, actual.AveragePricePerKwh);
    }

    [Fact]
    public void FindCheapest_TwoEqualWindows_ReturnsEarliest()
    {
        // Arrange
        var start = new DateTimeOffset(2026, 10, 26, 8, 0, 0, TimeSpan.Zero);

        var prices = new List<PricePoint>
        {
            new PricePoint(start, 5m),
            new PricePoint(start.AddMinutes(15), 1m),
            new PricePoint(start.AddMinutes(30), 1m),
            new PricePoint(start.AddMinutes(45), 5m),
            new PricePoint(start.AddMinutes(60), 1m),
            new PricePoint(start.AddMinutes(75), 1m),
            new PricePoint(start.AddMinutes(90), 5m)
        };

        var duration = TimeSpan.FromMinutes(30);

        // Act
        var actual = CheapestWindowFinder.FindCheapest(prices, duration, AllDay, now: start);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(start.AddMinutes(15), actual.Start);
        Assert.Equal(start.AddMinutes(45), actual.End);
        Assert.Equal(1m, actual.AveragePricePerKwh);
    }

    [Fact]
    public void FindCheapest_ZeroDuration_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var start = new DateTimeOffset(2026, 10, 26, 8, 0, 0, TimeSpan.Zero);

        var prices = new List<PricePoint>
        {
            new PricePoint(start, 1m),
            new PricePoint(start.AddMinutes(15), 1m)
        };

        var duration = TimeSpan.Zero;

        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => CheapestWindowFinder.FindCheapest(prices, duration, AllDay, now: DateTimeOffset.MinValue));
        Assert.Equal("duration", ex.ParamName);
    }

    [Fact]
    public void FindCheapest_InsufficientPrices_ThrowsArgumentException()
    {
        // Arrange
        var start = new DateTimeOffset(2026, 10, 26, 8, 0, 0, TimeSpan.Zero);

        var prices = new List<PricePoint>
        {
            new PricePoint(start, 1m),
            new PricePoint(start.AddMinutes(15), 2m)
        };

        var duration = TimeSpan.FromHours(1);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(
            () => CheapestWindowFinder.FindCheapest(prices, duration, AllDay, now: DateTimeOffset.MinValue));
        Assert.Equal("prices", ex.ParamName);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    public void FindCheapest_DurationNotMultipleOf15_ThrowsArgumentOutOfRangeException(int minutes)
    {
        // Arrange
        var start = new DateTimeOffset(2026, 10, 26, 8, 0, 0, TimeSpan.Zero);

        var prices = new List<PricePoint>
        {
            new PricePoint(start, 1m),
            new PricePoint(start.AddMinutes(15), 2m),
            new PricePoint(start.AddMinutes(30), 3m),
            new PricePoint(start.AddMinutes(45), 4m)
        };

        var duration = TimeSpan.FromMinutes(minutes);

        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => CheapestWindowFinder.FindCheapest(prices, duration, AllDay, now: DateTimeOffset.MinValue));
        Assert.Equal("duration", ex.ParamName);
    }

    [Fact]
    public void FindCheapest_CheapestIsBeforeNow_ReturnsLaterWindow()
    {
        // Arrange
        var start = new DateTimeOffset(2026, 10, 26, 8, 0, 0, TimeSpan.Zero);
        var prices = Quarters(start, 16, 1.00m);
        SetPrice(prices, start, start.AddHours(1), 0.10m);
        SetPrice(prices, start.AddHours(2), start.AddHours(3), 0.30m);
        var now = start.AddHours(1);

        // Act
        var actual = CheapestWindowFinder.FindCheapest(prices, TimeSpan.FromHours(1), AllDay, now);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(start.AddHours(2), actual.Start);
        Assert.Equal(0.30m, actual.AveragePricePerKwh);
    }

    [Fact]
    public void FindCheapest_CheapestIsInPeakHours_SkipsPeak()
    {
        // Arrange: 14:00-20:00 UTC is 15:00-21:00 Danish time (winter time, UTC+1)
        var start = new DateTimeOffset(2026, 10, 26, 14, 0, 0, TimeSpan.Zero);
        var prices = Quarters(start, 24, 1.00m);
        SetPrice(prices, start, start.AddHours(1), 0.50m);             // 15-16 Danish time
        SetPrice(prices, start.AddHours(2), start.AddHours(3), 0.10m); // 17-18 Danish time (peak)

        // Act
        var actual = CheapestWindowFinder.FindCheapest(prices, TimeSpan.FromHours(1), AllDay, now: start);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(start, actual.Start);
        Assert.Equal(0.50m, actual.AveragePricePerKwh);
    }

    [Fact]
    public void FindCheapest_CheapestIsOutsideAllowedHours_SkipsIt()
    {
        // Arrange: 00:00-12:00 UTC is 01:00-13:00 Danish time
        var start = new DateTimeOffset(2026, 10, 26, 0, 0, 0, TimeSpan.Zero);
        var prices = Quarters(start, 48, 1.00m);
        SetPrice(prices, start.AddHours(2), start.AddHours(3), 0.10m); // 03-04 Danish time
        SetPrice(prices, start.AddHours(8), start.AddHours(9), 0.30m); // 09-10 Danish time
        var allowedHours = new AllowedHours(7, 23);

        // Act
        var actual = CheapestWindowFinder.FindCheapest(prices, TimeSpan.FromHours(1), allowedHours, now: start);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(start.AddHours(8), actual.Start);
        Assert.Equal(0.30m, actual.AveragePricePerKwh);
    }

    [Fact]
    public void FindCheapest_NoAllowedWindow_ReturnsNull()
    {
        // Arrange: 16:00-20:00 UTC is 17:00-21:00 Danish time, so every window is in peak hours
        var start = new DateTimeOffset(2026, 10, 26, 16, 0, 0, TimeSpan.Zero);
        var prices = Quarters(start, 16, 1.00m);

        // Act
        var actual = CheapestWindowFinder.FindCheapest(prices, TimeSpan.FromHours(1), AllDay, now: start);

        // Assert
        Assert.Null(actual);
    }

    [Fact]
    public void FindCheapest_On25Oct2026_UsesWinterTimeForPeak()
    {
        // Arrange: the clocks go back on 25.10.2026, so the day has 100 quarters
        var prices = DanishDay(new DateTime(2026, 10, 25), 1.00m);
        Assert.Equal(100, prices.Count);

        var fourPmDanish = new DateTimeOffset(2026, 10, 25, 15, 0, 0, TimeSpan.Zero);
        SetPrice(prices, fourPmDanish, fourPmDanish.AddHours(1), 0.20m);
        SetPrice(prices, fourPmDanish.AddHours(1), fourPmDanish.AddHours(2), 0.10m); // 17-18 Danish time (peak)

        // Act
        var actual = CheapestWindowFinder.FindCheapest(prices, TimeSpan.FromHours(1), AllDay, now: prices[0].Start);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(fourPmDanish, actual.Start);
        Assert.Equal(0.20m, actual.AveragePricePerKwh);
    }

    [Fact]
    public void FindCheapest_On29Mar2026_ReturnsThreeRealHours()
    {
        // Arrange: the clocks go forward on 29.3.2026, so the day has 92 quarters
        var prices = DanishDay(new DateTime(2026, 3, 29), 1.00m);
        Assert.Equal(92, prices.Count);

        var nightStart = new DateTimeOffset(2026, 3, 29, 0, 0, 0, TimeSpan.Zero); // 01:00 Danish time
        SetPrice(prices, nightStart, nightStart.AddHours(3), 0.10m);              // until 05:00 Danish time

        // Act
        var actual = CheapestWindowFinder.FindCheapest(prices, TimeSpan.FromHours(3), AllDay, now: prices[0].Start);

        // Assert
        Assert.NotNull(actual);
        Assert.Equal(nightStart, actual.Start);
        Assert.Equal(nightStart.AddHours(3), actual.End);
    }

    private static List<PricePoint> Quarters(DateTimeOffset firstStart, int count, decimal price)
    {
        var prices = new List<PricePoint>();

        for (int i = 0; i < count; i++)
        {
            prices.Add(new PricePoint(firstStart.AddMinutes(15 * i), price));
        }

        return prices;
    }

    private static List<PricePoint> DanishDay(DateTime date, decimal price)
    {
        DateTimeOffset start = TimeZoneInfo.ConvertTimeToUtc(date.Date, DanishTime.Zone);
        DateTimeOffset end = TimeZoneInfo.ConvertTimeToUtc(date.Date.AddDays(1), DanishTime.Zone);
        int count = (int)((end - start).TotalMinutes / 15);

        return Quarters(start, count, price);
    }

    private static void SetPrice(List<PricePoint> prices, DateTimeOffset from, DateTimeOffset to, decimal price)
    {
        for (int i = 0; i < prices.Count; i++)
        {
            if (prices[i].Start >= from && prices[i].Start < to)
            {
                prices[i] = prices[i] with { PricePerKwh = price };
            }
        }
    }
}