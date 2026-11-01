using System.Text.Json;
using Elwise.Infrastructure.EnergiDataService;
using Xunit;

namespace Elwise.Tests.EnergiDataService;

public class DayAheadPricesResponseTests
{
    [Fact]
    public void Deserialize_RealJson_ReadsAllRecords()
    {
        // Arrange: the first two records from the real response on 31.10.2026
        var json = """
            {
              "total": 2,
              "dataset": "DayAheadPrices",
              "records": [
                { "TimeUTC": "2026-10-31T22:45:00", "TimeDK": "2026-10-31T23:45:00", "PriceArea": "DK1", "DayAheadPriceEUR": 232.259995, "DayAheadPriceDKK": 1735.795073 },
                { "TimeUTC": "2026-10-31T22:30:00", "TimeDK": "2026-10-31T23:30:00", "PriceArea": "DK1", "DayAheadPriceEUR": 244.009995, "DayAheadPriceDKK": 1823.608698 }
              ]
            }
            """;

        // Act
        var response = JsonSerializer.Deserialize<DayAheadPricesResponse>(json);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(2, response.Records.Count);
        Assert.Equal(new DateTime(2026, 10, 31, 22, 45, 0), response.Records[0].TimeUtc);
        Assert.Equal(1735.795073m, response.Records[0].PriceDkkPerMwh);
        Assert.Equal(1823.608698m, response.Records[1].PriceDkkPerMwh);
    }
}