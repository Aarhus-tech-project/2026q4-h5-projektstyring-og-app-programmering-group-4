using Elwise.Core.Prices;
using Elwise.Core.Scheduling;
using Xunit;

namespace Elwise.Tests.Scheduling;

public class WindowPriceTests
{
    private static readonly DateTimeOffset Start = new(2026, 11, 3, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void StartingAt_UsesTheQuarterThatContainsTheTime()
    {
        // Arrange
        var prices = Prices(1.00m, 2.00m, 3.00m, 4.00m);

        // Act: 00:20 is inside the quarter that starts at 00:15
        PriceWindow? window = WindowPrice.StartingAt(prices, Start.AddMinutes(20), TimeSpan.FromMinutes(30));

        // Assert
        Assert.NotNull(window);
        Assert.Equal(Start.AddMinutes(15), window.Start);
        Assert.Equal(2.50m, window.AveragePricePerKwh);
    }
    [Fact]
    public void StartingAt_NotEnoughPrices_ReturnsNull()
    {
        // Arrange
        var prices = Prices(1.00m, 2.00m);

        // Act
        PriceWindow? window = WindowPrice.StartingAt(prices, Start.AddMinutes(15), TimeSpan.FromMinutes(30));

        // Assert
        Assert.Null(window);
    }


    private static List<PricePoint> Prices(params decimal[] values) =>

        values.Select((value, index) => new PricePoint(Start.AddMinutes(15 * index), value)).ToList();
}