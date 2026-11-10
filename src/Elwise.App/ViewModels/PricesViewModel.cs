using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Elwise.App.Settings;
using Elwise.Core.Prices;
using Elwise.Core.Scheduling;
using Elwise.Core.Time;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;

namespace Elwise.App.ViewModels;

/// <summary>
/// Loads today's and tomorrow's prices and finds the cheapest time window.
/// </summary>
public sealed class PricesViewModel : INotifyPropertyChanged
{
    private const int ReminderId = 1;
    private static readonly CultureInfo Danish = CultureInfo.GetCultureInfo("da-DK");

    private readonly IPriceSource _priceSource;
    private readonly AppSettings _settings;
    private string _cheapestText = "";
    private PriceWindow? _cheapest;
    private string _reminderText = "";

    public PricesViewModel(IPriceSource priceSource, AppSettings settings)
    {
        _priceSource = priceSource;
        _settings = settings;
        RemindCommand = new Command(async () => await RemindAsync(), () => _cheapest is not null);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<PriceGroup> Days { get; } = new();
    public Command RemindCommand { get; }

    public string CheapestText
    {
        get => _cheapestText;
        private set
        {
            _cheapestText = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CheapestText)));
        }
    }

    public string ReminderText
    {
        get => _reminderText;
        private set
        {
            _reminderText = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ReminderText)));
        }
    }

    public async Task LoadAsync()
    {
        DateOnly today = DateOnly.FromDateTime(DanishTime.FromUtc(DateTimeOffset.UtcNow).DateTime);
        DateOnly tomorrow = today.AddDays(1);
        PriceArea area = _settings.Area;

        IReadOnlyList<PricePoint> todayPrices = await _priceSource.GetPricesAsync(area, today);
        IReadOnlyList<PricePoint> tomorrowPrices = await GetTomorrowPricesAsync(area, tomorrow);

        Days.Clear();
        Days.Add(new PriceGroup($"I dag · {area}", ToRows(todayPrices)));

        if (tomorrowPrices.Count > 0)
        {
            Days.Add(new PriceGroup($"I morgen · {area}", ToRows(tomorrowPrices)));
        }

        var allPrices = new List<PricePoint>(todayPrices);
        allPrices.AddRange(tomorrowPrices);

        _cheapest = allPrices.Count == 0
            ? null
            : CheapestWindowFinder.FindCheapest(allPrices, _settings.Duration, _settings.AllowedHours, DateTimeOffset.UtcNow);
        CheapestText = DescribeCheapest(_cheapest, today);
        RemindCommand.ChangeCanExecute();
    }
    private async Task<IReadOnlyList<PricePoint>> GetTomorrowPricesAsync(PriceArea area, DateOnly tomorrow)
    {
        try
        {
            return await _priceSource.GetPricesAsync(area, tomorrow);
        }
        catch (HttpRequestException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Tomorrow's prices failed: {ex.Message}");
            return [];
        }
    }

    private async Task RemindAsync()
    {
        PriceWindow? window = _cheapest;

        if (window is null)
        {
            return;
        }

        try
        {
            if (await LocalNotificationCenter.Current.AreNotificationsEnabled() == false)
            {
            await LocalNotificationCenter.Current.RequestNotificationPermission();
        }

        var reminder = new NotificationRequest
        {
            NotificationId = ReminderId,
            Title = "Start opvasken nu",
            Description = CheapestText,
            Schedule =
            {
                NotifyTime = window.Start
            }
        };
        await LocalNotificationCenter.Current.Show(reminder);

        string time = DanishTime.FromUtc(window.Start).ToString("HH:mm", Danish);
        ReminderText = $"Påmindelse sat til kl. {time}.";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Reminder failed: {ex.Message}");
            ReminderText = "Påmindelsen kunne ikke sættes på denne enhed.";
        }
    }

    private static string DescribeCheapest(PriceWindow? cheapest, DateOnly today)
    {
        if (cheapest is null)
        { 
            return "Intet tidsrum passer til dine indstillinger.";
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