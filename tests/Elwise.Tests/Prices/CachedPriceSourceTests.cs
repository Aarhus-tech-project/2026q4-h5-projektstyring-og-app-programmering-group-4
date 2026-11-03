using Elwise.Core.Prices;
using Xunit;

namespace Elwise.Tests.Prices;

public class CachedPriceSourceTests
{
    private static readonly DateOnly Day = new(2026, 11, 3);

    [Fact]
    public async Task GetPricesAsync_SameDayTwice_FetchesOnce()
    {
        // Arrange
        var inner = new CountingPriceSource(OnePrice());
        var cached = new CachedPriceSource(inner, new FakeTimeProvider());

        // Act
        await cached.GetPricesAsync(PriceArea.DK1, Day);
        await cached.GetPricesAsync(PriceArea.DK1, Day);

        // Assert
        Assert.Equal(1, inner.Calls);
    }

    [Fact]
    public async Task GetPricesAsync_EmptyResult_FetchesAgainAfterTenMinutes()
    {
        // Arrange
        var inner = new CountingPriceSource([]);
        var clock = new FakeTimeProvider();
        var cached = new CachedPriceSource(inner, clock);

        // Act: fetch at 0, 5 and 11 minutes
        await cached.GetPricesAsync(PriceArea.DK1, Day);
        clock.UtcNow = clock.UtcNow.AddMinutes(5);
        await cached.GetPricesAsync(PriceArea.DK1, Day);
        clock.UtcNow = clock.UtcNow.AddMinutes(6);
        await cached.GetPricesAsync(PriceArea.DK1, Day);

        // Assert: fetched at 0 and 11 minutes, not at 5 minutes
        Assert.Equal(2, inner.Calls);
    }

    private static List<PricePoint> OnePrice() =>
        [new PricePoint(new DateTimeOffset(2026, 11, 2, 23, 0, 0, TimeSpan.Zero), 1.00m)];

    private sealed class CountingPriceSource : IPriceSource
    {
        private readonly IReadOnlyList<PricePoint> _prices;

        public CountingPriceSource(IReadOnlyList<PricePoint> prices)
        {
            _prices = prices;
        }

        public int Calls { get; private set; }

        public Task<IReadOnlyList<PricePoint>> GetPricesAsync(
            PriceArea area,
            DateOnly day,
            CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(_prices);
        }
    }

    private sealed class FakeTimeProvider : TimeProvider
    {
        public DateTimeOffset UtcNow { get; set; } = new(2026, 11, 3, 10, 0, 0, TimeSpan.Zero);

        public override DateTimeOffset GetUtcNow() => UtcNow;
    }
}