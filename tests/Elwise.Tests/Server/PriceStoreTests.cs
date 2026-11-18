using Elwise.Api.Data;
using Elwise.Api.Prices;
using Elwise.Core.Prices;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Elwise.Tests.Server;

public sealed class PriceStoreTests : IDisposable
{
    // 23:00 UTC on 17.11 is 00:00 Danish time on 18.11
    private static readonly DateOnly Day = new(2026, 11, 18);

    private readonly SqliteConnection _connection;
    private readonly ElwiseDbContext _db;

    public PriceStoreTests()
    {
        // A SQLite database in memory. It lives as long as the connection is open.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ElwiseDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new ElwiseDbContext(options);
        _db.Database.EnsureCreated();
    }

    [Fact]
    public async Task GetPricesAsync_DayMissing_FetchesAndSaves()
    {
        // Arrange
        var source = new CountingPriceSource(TwoPrices());
        var store = new PriceStore(_db, source);

        // Act
        IReadOnlyList<PricePoint> prices = await store.GetPricesAsync(PriceArea.DK1, Day, 1);

        // Assert
        Assert.Equal(1, source.Calls);
        Assert.Equal(2, prices.Count);
        Assert.Equal(2, await _db.Prices.CountAsync());
    }

    [Fact]
    public async Task GetPricesAsync_SameDayTwice_FetchesOnce()
    {
        // Arrange
        var source = new CountingPriceSource(TwoPrices());
        var store = new PriceStore(_db, source);

        // Act
        await store.GetPricesAsync(PriceArea.DK1, Day, 1);
        IReadOnlyList<PricePoint> prices = await store.GetPricesAsync(PriceArea.DK1, Day, 1);

        // Assert: the second time, the prices come from the database
        Assert.Equal(1, source.Calls);
        Assert.Equal(new DateTimeOffset(2026, 11, 17, 23, 0, 0, TimeSpan.Zero), prices[0].Start);
        Assert.Equal(1.25m, prices[0].PricePerKwh);
    }

    [Fact]
    public async Task GetPricesAsync_OtherArea_FetchesAgain()
    {
        // Arrange
        var source = new CountingPriceSource(TwoPrices());
        var store = new PriceStore(_db, source);

        // Act
        await store.GetPricesAsync(PriceArea.DK1, Day, 1);
        await store.GetPricesAsync(PriceArea.DK2, Day, 1);

        // Assert
        Assert.Equal(2, source.Calls);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private static List<PricePoint> TwoPrices() =>
    [
        new PricePoint(new DateTimeOffset(2026, 11, 17, 23, 0, 0, TimeSpan.Zero), 1.25m),
        new PricePoint(new DateTimeOffset(2026, 11, 17, 23, 15, 0, TimeSpan.Zero), 0.75m)
    ];

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
            DateOnly firstDay,
            int dayCount,
            CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(_prices);
        }
    }
}