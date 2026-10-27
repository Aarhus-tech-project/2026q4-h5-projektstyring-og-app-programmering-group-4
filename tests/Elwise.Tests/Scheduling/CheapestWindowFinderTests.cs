using Elwise.Core.Prices;
using Elwise.Core.Scheduling;
using Xunit;

namespace Elwise.Tests.Scheduling;

public class CheapestWindowFinderTests
{
    [Fact]
    public void FindCheapest_ReturnsCheapestTwoHourWindow()
    {
        // Arrange
        var start = new DateTimeOffset(
            2026, 10, 26, 8, 0, 0, TimeSpan.Zero
        );

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
        var actual = CheapestWindowFinder.FindCheapest(prices, duration);
        Assert.Equal(start.AddMinutes(15), actual.Start);
        Assert.Equal(start.AddMinutes(135), actual.End);
        Assert.Equal(0.50m, actual.AveragePricePerKwh);

    }
}