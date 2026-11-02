using System.Collections.ObjectModel;
using System.Globalization;
using Elwise.Core.Prices;
using Elwise.Core.Time;

namespace Elwise.App.ViewModels;

/// <summary>
/// Loads today's prices and prepares them for the price list.
/// </summary>
public sealed class PricesViewModel
{
    private static readonly CultureInfo Danish = CultureInfo.GetCultureInfo("da-DK");

    private readonly IPriceSource _priceSource;

    public PricesViewModel(IPriceSource priceSource)
    {
        _priceSource = priceSource;
    }

    public ObservableCollection<PriceRow> Prices { get; } = new();

    public async Task LoadAsync()
    {
        DateOnly today = DateOnly.FromDateTime(DanishTime.FromUtc(DateTimeOffset.UtcNow).DateTime);

        IReadOnlyList<PricePoint> prices = await _priceSource.GetPricesAsync(PriceArea.DK1, today);

        Prices.Clear();

        foreach (PricePoint price in prices)
        {
            string time = DanishTime.FromUtc(price.Start).ToString("HH:mm", Danish);
            string kroner = price.PricePerKwh.ToString("0.00", Danish) + " kr.";

            Prices.Add(new PriceRow(time, kroner));
        }
    }
}