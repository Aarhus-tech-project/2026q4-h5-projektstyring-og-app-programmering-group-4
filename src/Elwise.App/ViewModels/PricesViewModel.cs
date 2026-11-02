using System.Collections.ObjectModel;
using System.Globalization;
using Elwise.Core.Prices;
using Elwise.Core.Time;

namespace Elwise.App.ViewModels;

/// <summary>
/// Loads today's and tomorrow's prices and prepares them for the price list.
/// </summary>
public sealed class PricesViewModel
{
    private static readonly CultureInfo Danish = CultureInfo.GetCultureInfo("da-DK");

    private readonly IPriceSource _priceSource;

    public PricesViewModel(IPriceSource priceSource)
    {
        _priceSource = priceSource;
    }

    public ObservableCollection<PriceGroup> Days { get; } = new();

    public async Task LoadAsync()
    {
        DateOnly today = DateOnly.FromDateTime(DanishTime.FromUtc(DateTimeOffset.UtcNow).DateTime);
        DateOnly tomorrow = today.AddDays(1);

        IReadOnlyList<PricePoint> todayPrices = await _priceSource.GetPricesAsync(PriceArea.DK1, today);
        IReadOnlyList<PricePoint> tomorrowPrices = await _priceSource.GetPricesAsync(PriceArea.DK1, tomorrow);

        Days.Clear();
        Days.Add(new PriceGroup("I dag", ToRows(todayPrices)));

        if (tomorrowPrices.Count > 0)
        {
            Days.Add(new PriceGroup("I morgen", ToRows(tomorrowPrices)));
        }
    }

    private static List<PriceRow> ToRows(IReadOnlyList<PricePoint> prices)
    {
        var rows = new List<PriceRow>();

        foreach (PricePoint price in prices)
        {
            string time = DanishTime.FromUtc(price.Start).ToString("HH:mm", Danish);
            string kroner = price.PricePerKwh.ToString("0.00", Danish) + " kr.";

            rows.Add(new PriceRow(time, kroner));
        }

        return rows;
    }
}