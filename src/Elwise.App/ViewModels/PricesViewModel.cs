using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Elwise.Core.Prices;
using Elwise.Core.Scheduling;
using Elwise.Core.Time;

namespace Elwise.App.ViewModels;

/// <summary>
/// Loads today's and tomorrow's prices and finds the cheapest time window.
/// </summary>
public sealed class PricesViewModel : INotifyPropertyChanged
{
    private static readonly CultureInfo Danish = CultureInfo.GetCultureInfo("da-DK");
    private static readonly TimeSpan Duration = TimeSpan.FromHours(2);
    private static readonly AllowedHours AllDay = new(0, 24);

    private readonly IPriceSource _priceSource;
    private string _cheapestText = "";

    public PricesViewModel(IPriceSource priceSource)
    {
        _priceSource = priceSource;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<PriceGroup> Days { get; } = new();

    public string CheapestText
    {
        get => _cheapestText;
        private set
        {
            _cheapestText = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CheapestText)));
        }
    }

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

        var allPrices = new List<PricePoint>(todayPrices);
        allPrices.AddRange(tomorrowPrices);

        CheapestText = DescribeCheapest(allPrices, today);
    }

    private static string DescribeCheapest(IReadOnlyList<PricePoint> prices, DateOnly today)
    {
        if (prices.Count == 0)
        {
            return "Ingen priser endnu.";
        }

        PriceWindow? cheapest = CheapestWindowFinder.FindCheapest(prices, Duration, AllDay, DateTimeOffset.UtcNow);

        if (cheapest is null)
        {
            return "Intet tidsrum passer til reglerne.";
        }

        DateTimeOffset start = DanishTime.FromUtc(cheapest.Start);
        DateTimeOffset end = DanishTime.FromUtc(cheapest.End);

        string day = DateOnly.FromDateTime(start.DateTime) == today ? "I dag" : "I morgen";
        string from = start.ToString("HH:mm", Danish);
        string to = end.ToString("HH:mm", Danish);
        string price = cheapest.AveragePricePerKwh.ToString("0.00", Danish);

        return $"{day} kl. {from}–{to} · {price} kr./kWh i gennemsnit";
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