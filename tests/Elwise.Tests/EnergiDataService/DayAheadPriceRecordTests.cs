using Elwise.Infrastructure.EnergiDataService;
using Xunit;

namespace Elwise.Tests.EnergiDataService;

public class DayAheadPriceRecordTests
{
    [Fact]
    public void ToPricePoint_ConvertsToUtcAndDkkPerKwh()
    {
        // Arrange: the first price in the real JSON (31.10.2026, 23:45 Danish time)
        var record = new DayAheadPriceRecord(new DateTime(2026, 10, 31, 22, 45, 0), 1735.795073m);

        // Act
        var actual = record.ToPricePoint();

        // Assert
        Assert.Equal(new DateTimeOffset(2026, 10, 31, 22, 45, 0, TimeSpan.Zero), actual.Start);
        Assert.Equal(1.735795073m, actual.PricePerKwh);
    }
}