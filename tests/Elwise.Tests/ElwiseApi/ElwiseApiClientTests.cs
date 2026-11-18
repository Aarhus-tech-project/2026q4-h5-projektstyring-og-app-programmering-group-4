using System.Net;
using System.Text;
using Elwise.Core.Prices;
using Elwise.Infrastructure.ElwiseApi;
using Xunit;

namespace Elwise.Tests.ElwiseApi;

public class ElwiseApiClientTests
{
    private const string Json = """
        [
          { "start": "2026-11-17T23:00:00+00:00", "pricePerKwh": 0.79 },
          { "start": "2026-11-17T23:15:00+00:00", "pricePerKwh": 0.80 }
        ]
        """;
    [Fact]
    public async Task GetPricesAsync_ReturnsPricePoints()
    {
        // Arrange
        var handler = new FakeHandler(Json);
        var client = new ElwiseApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") });
        // Act
        IReadOnlyList<PricePoint> prices = await client.GetPricesAsync(PriceArea.DK1, new DateOnly(2026, 11, 18), 2);

        // Assert
        Assert.Equal(2, prices.Count);
        Assert.Equal(new DateTimeOffset(2026, 11, 17, 23, 0, 0, TimeSpan.Zero), prices[0].Start);
        Assert.Equal(0.79m, prices[0].PricePerKwh);
    }

    [Fact]
      public async Task GetPricesAsync_AsksForTheRightDaysAndArea()
    {
        // Arrange
        var handler = new FakeHandler(Json);
        var client = new ElwiseApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://example.test/") });

        // Act
        await client.GetPricesAsync(PriceArea.DK2, new DateOnly(2026, 11, 18), 2);

        // Assert
        Assert.Equal(
            "https://example.test/api/prices?area=DK2&firstDay=2026-11-18&dayCount=2",
            handler.RequestUri?.ToString());
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