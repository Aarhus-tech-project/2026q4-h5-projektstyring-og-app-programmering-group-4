using Elwise.Core.Prices;
using Elwise.Core.Scheduling;
using System.Reflection.Metadata.Ecma335;
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

    [Fact]
    public void FindCheapest_CheapestWindowMiddle_ReturnsMiddleWindow()
    {
        // Arrange
        var start = new DateTimeOffset(
            2026, 10, 26, 8, 0, 0, TimeSpan.Zero
        );

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

        var actual = CheapestWindowFinder.FindCheapest(prices, duration);

        // Assert

        Assert.Equal(start.AddMinutes(30), actual.Start);
        Assert.Equal(start.AddMinutes(90), actual.End);
        Assert.Equal(1m, actual.AveragePricePerKwh);

    }

    [Fact]
    public void FindCheapest_TwoEqualWindows_ReturnsEarliest()
    {
        //Arrange
        var start = new DateTimeOffset(
            2026, 10, 26, 8, 0, 0, TimeSpan.Zero
        );

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
        //Act
        var actual = CheapestWindowFinder.FindCheapest(prices, duration);
        //Assert
        Assert.Equal(start.AddMinutes(15), actual.Start);
        Assert.Equal(start.AddMinutes(45), actual.End);
        Assert.Equal(1m, actual.AveragePricePerKwh);
    }

    [Fact]
    public void FindCheapest_ZeroDuration_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
         var start = new DateTimeOffset(
            2026, 10, 26, 8, 0, 0, TimeSpan.Zero);

         var prices = new List<PricePoint>
         {
          new PricePoint(start,1m),
          new PricePoint(start.AddMinutes(15),1m),
         };

        var duration = TimeSpan.Zero;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CheapestWindowFinder.FindCheapest(prices, duration)
        );

    }

    [Fact]
    public void FindCheapest_InsufficientPrices_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var start = new DateTimeOffset(
            2026, 10, 26, 8, 0, 0, TimeSpan.Zero);
        var prices = new List<PricePoint>
        {
            new PricePoint(start,1m),
            new PricePoint(start.AddMinutes(15),2m),
        };
        var duration = TimeSpan.FromHours(1);
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => CheapestWindowFinder.FindCheapest(prices, duration)
        );
    }

    [Theory]
    [InlineData(10)]
    [InlineData(50)]
    public void FindCheapest_DurationNotMultipleOf15_ThrowsArgumentOutOfRange(int munutes)
    {
        //Arrange
        var start = new DateTimeOffset(
            2026, 10, 26, 8, 0, 0, TimeSpan.Zero);

        var prices = new List<PricePoint>
        {
            new PricePoint(start,1m),
            new PricePoint(start.AddMinutes(15),2m),
            new PricePoint(start.AddMinutes(30),3m),
            new PricePoint(start.AddMinutes(45),4m),
        };
        var duration = TimeSpan.FromMinutes(munutes);
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CheapestWindowFinder.FindCheapest(prices, duration)
        );
    }
}