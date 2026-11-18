using Elwise.Core.Prices;
using Xunit;

namespace Elwise.Tests.Prices;

public class FallbackPriceSourceTests
{
    private static readonly DateOnly Day = new(2026, 11, 18);

    [Fact]
    public async Task GetPricesAsync_FirstWorks_DoesNotUseFallback()
    {
        // Arrange
        var first = new FakePriceSource(fails: false);
        var fallback = new FakePriceSource(fails: false);
        var source = new FallbackPriceSource(first, fallback);

        // Act
        await source.GetPricesAsync(PriceArea.DK1, Day, 2);

        // Assert
        Assert.Equal(1, first.Calls);
        Assert.Equal(0, fallback.Calls);
    }

    [Fact]
    public async Task GetPricesAsync_FirstFails_UsesFallback()
    {
        // Arrange
        var first = new FakePriceSource(fails: true);
        var fallback = new FakePriceSource(fails: false);
        var source = new FallbackPriceSource(first, fallback);

        // Act
        IReadOnlyList<PricePoint> prices = await source.GetPricesAsync(PriceArea.DK1, Day, 2);

        // Assert
        Assert.Equal(1, fallback.Calls);
        Assert.Single(prices);
    }
    private sealed class FakePriceSource : IPriceSource
    {
        private readonly bool _fails;

       
        public FakePriceSource(bool fails)
        {
            _fails = fails;
        }

       
        public int Calls { get; private set; }


      
        public Task<IReadOnlyList<PricePoint>> GetPricesAsync(
            PriceArea area,
            DateOnly firstDay,
            int dayCount,
            CancellationToken cancellationToken = default)
        {
            Calls++;

            if (_fails)
            {
                throw new HttpRequestException("The server is offline.");
            }

            IReadOnlyList<PricePoint> prices =
                [new PricePoint(new DateTimeOffset(2026, 11, 17, 23, 0, 0, TimeSpan.Zero), 1.00m)];

            return Task.FromResult(prices);
        

        }
    }
}