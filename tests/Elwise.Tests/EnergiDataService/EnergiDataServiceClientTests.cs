using System.Net;
using System.Text;
using Elwise.Core.Prices;
using Elwise.Infrastructure.EnergiDataService;
using Xunit;

namespace Elwise.Tests.EnergiDataService;

public class EnergiDataServiceClientTests
{
    // 23:00 UTC is 00:00 Danish time, the first quarter of 1.11.2026
    private const string Json = """
        {
          "records": [
            { "TimeUTC": "2026-10-31T23:00:00", "DayAheadPriceDKK": 1000.0 },
            { "TimeUTC": "2026-10-31T23:15:00", "DayAheadPriceDKK": 500.0 }
          ]
        }
        """;

    [Fact]
    public async Task GetPricesAsync_ReturnsPricePoints()
    {
        // Arrange
        var handler = new FakeHandler (Json);
        var client = new EnergiDataServiceClient(new HttpClient(handler));

        // Act 
        var prices = await client.GetPricesAsync(PriceArea.DK1, new DateOnly(2026, 11, 1));

        // Assert
        Assert.Equal(2, prices.Count);
        Assert.Equal(new DateTimeOffset(2026, 10, 31, 23, 0, 0, TimeSpan.Zero), prices[0].Start);
        Assert.Equal(1.0m, prices[0].PricePerKwh);
        Assert.Equal(0.5m, prices[1].PricePerKwh);

    }

    [Fact]
    public async Task GetPricesAsync_AsksForTheRightDayAndArea()
    {
                // Arrange
        var handler = new FakeHandler(Json);
        var client = new EnergiDataServiceClient(new HttpClient(handler));

        // Act
        await client.GetPricesAsync(PriceArea.DK1, new DateOnly(2026, 11, 1));

        // Assert
        Assert.NotNull(handler.RequestUri);
        Assert.Equal(
            """?start=2026-11-01&end=2026-11-02&filter={"PriceArea":["DK1"]}&sort=TimeUTC&limit=0""",
            Uri.UnescapeDataString(handler.RequestUri.Query));
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly string _json;

        public FakeHandler(string json)
        {
            _json = json;
        }

        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;

            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(_json, Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }
}
