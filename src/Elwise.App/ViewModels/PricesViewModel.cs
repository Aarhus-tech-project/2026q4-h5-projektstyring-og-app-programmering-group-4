using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Elwise.App.Charts;
using Elwise.App.Settings;
using Elwise.Core.Prices;
using Elwise.Core.Scheduling;
using Elwise.Core.Time;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;

namespace Elwise.App.ViewModels;

/// <summary>
/// Loads today's and tomorrow's prices and finds the cheapest time to start the dishwasher.
/// </summary>
public sealed class PricesViewModel : INotifyPropertyChanged
{
    private const int ReminderId = 1;
    private static readonly CultureInfo Danish = CultureInfo.GetCultureInfo("da-DK");

    private readonly IPriceSource _priceSource;
    private readonly AppSettings _settings;
    private PriceWindow? _cheapest;
    private IReadOnlyList<PriceGroup> _days = [];
    private string _startTimeText = "";
    private string _startWhenText = "Henter priser …";
    private string _detailText = "";
    private string _savingsText = "";
    private string _remindButtonText = "Påmind mig";
    private string _reminderText = "";
    private string _priceNoteText = "";
    private string _cheapestText = "";

    public PricesViewModel(IPriceSource priceSource, AppSettings settings)
    {
        _priceSource = priceSource;
        _settings = settings;
        RemindCommand = new Command(async () => await RemindAsync(), () => _cheapest is not null);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public Command RemindCommand { get; }

    /// <summary>
    /// Draws the price chart on the main page.
    /// </summary>
    public PriceChartDrawable Chart { get; } = new();

    /// <summary>
    /// The price list with one group per day. Every load gives a new list.
    /// </summary>
    public IReadOnlyList<PriceGroup> Days
    {
        get => _days;
        private set
        {
            _days = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// When to start the dishwasher, for example "03.00".
    /// </summary>
    public string StartTimeText { get => _startTimeText; private set => SetText(ref _startTimeText, value); }

    /// <summary>
    /// The day and the time until the start, for example "I nat, om 11 t 45 min".
    /// </summary>
    public string StartWhenText { get => _startWhenText; private set => SetText(ref _startWhenText, value); }

    /// <summary>
    /// When the program is done and its average price.
    /// </summary>
    public string DetailText { get => _detailText; private set => SetText(ref _detailText, value); }

    /// <summary>
    /// How much cheaper the cheapest time is than starting now, or empty.
    /// </summary>
    public string SavingsText
    {
        get => _savingsText;
        private set
        {
            SetText(ref _savingsText, value);
            OnPropertyChanged(nameof(HasSavings));
        }
    }

    public bool HasSavings => SavingsText.Length > 0;

    public string RemindButtonText { get => _remindButtonText; private set => SetText(ref _remindButtonText, value); }

    public string ReminderText { get => _reminderText; private set => SetText(ref _reminderText, value); }

    /// <summary>
    /// The price area and what the prices include, shown under the chart.
    /// </summary>
    public string PriceNoteText { get => _priceNoteText; private set => SetText(ref _priceNoteText, value); }

    /// <summary>
    /// The cheapest time in one sentence. Used as the text of the reminder.
    /// </summary>
    public string CheapestText { get => _cheapestText; private set => SetText(ref _cheapestText, value); }

    public async Task LoadAsync()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        DateOnly today = DanishDay(now);
        PriceArea area = _settings.Area;

        IReadOnlyList<PricePoint> prices = await _priceSource.GetPricesAsync(area, today, 2);

        var todayPrices = new List<PricePoint>();
        var tomorrowPrices = new List<PricePoint>();

        foreach (PricePoint price in prices)
        {
            if (DanishDay(price.Start) == today)
            {
                todayPrices.Add(price);
            }
            else
            {
                tomorrowPrices.Add(price);
            }
        }

        _cheapest = prices.Count == 0
            ? null
            : CheapestWindowFinder.FindCheapest(prices, _settings.Duration, _settings.AllowedHours, now);

        var days = new List<PriceGroup> { new PriceGroup("I dag", ToRows(todayPrices)) };

        if (tomorrowPrices.Count > 0)
        {
            days.Add(new PriceGroup("I morgen", ToRows(tomorrowPrices)));
        }

        Days = days;
        PriceNoteText = $"Spotpris i {area} uden nettarif, afgifter og moms. Kilde: Energi Data Service.";
        ShowCheapest(prices, today, now);

        Chart.Prices = prices;
        Chart.Cheapest = _cheapest;
        Chart.Now = now;

        RemindCommand.ChangeCanExecute();
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

    private void ShowCheapest(IReadOnlyList<PricePoint> prices, DateOnly today, DateTimeOffset now)
    {
        if (_cheapest is null)
        {
            StartTimeText = "–";
            StartWhenText = "Intet tidsrum passer til dine indstillinger.";
            DetailText = "";
            SavingsText = "";
            RemindButtonText = "Påmind mig";
            CheapestText = "";
            return;
        }

        DateTimeOffset start = DanishTime.FromUtc(_cheapest.Start);
        string startTime = start.ToString("HH:mm", Danish);
        string endTime = DanishTime.FromUtc(_cheapest.End).ToString("HH:mm", Danish);
        string price = _cheapest.AveragePricePerKwh.ToString("0.00", Danish);

        StartTimeText = startTime;
        StartWhenText = $"{DayWord(start, today)}, om {Countdown(_cheapest.Start - now)}";
        DetailText = $"Færdig kl. {endTime}. I gennemsnit {price} kr. pr. kWh.";
        SavingsText = DescribeSavings(_cheapest, prices, now);
        RemindButtonText = $"Påmind mig kl. {startTime}";
        CheapestText = $"Start kl. {startTime}, færdig kl. {endTime}. I gennemsnit {price} kr. pr. kWh.";
    }

    private string DescribeSavings(PriceWindow cheapest, IReadOnlyList<PricePoint> prices, DateTimeOffset now)
    {
        PriceWindow? startNow = WindowPrice.StartingAt(prices, now, _settings.Duration);

        if (startNow is null || startNow.AveragePricePerKwh <= 0)
        {
            return "";
        }

        decimal percent = (startNow.AveragePricePerKwh - cheapest.AveragePricePerKwh)
            / startNow.AveragePricePerKwh * 100m;

        return percent >= 1m ? $"{percent:0} % billigere end at starte nu" : "";
    }

    private List<PriceRow> ToRows(IReadOnlyList<PricePoint> prices)
    {
        var rows = new List<PriceRow>();

        foreach (PricePoint price in prices)
        {
            string time = DanishTime.FromUtc(price.Start).ToString("HH:mm", Danish);
            string kroner = price.PricePerKwh.ToString("0.00", Danish) + " kr.";
            bool isInWindow = _cheapest is not null && price.Start >= _cheapest.Start && price.Start < _cheapest.End;

            rows.Add(new PriceRow(time, kroner, isInWindow));
        }

        return rows;
    }
    private static DateOnly DanishDay(DateTimeOffset time) =>
        DateOnly.FromDateTime(DanishTime.FromUtc(time).DateTime);

    private static string DayWord(DateTimeOffset danishStart, DateOnly today)
    {
        if (DateOnly.FromDateTime(danishStart.DateTime) == today)
        {
            return "I dag";
        }

        return danishStart.Hour < 6 ? "I nat" : "I morgen";
    }
    private static string Countdown(TimeSpan untilStart)
    {
        int totalMinutes = Math.Max(0, (int)Math.Ceiling(untilStart.TotalMinutes));
        int hours = totalMinutes / 60;
        int minutes = totalMinutes % 60;

        if (hours == 0)
        {
            return $"{minutes} min";
        }

        return minutes == 0 ? $"{hours} t" : $"{hours} t {minutes} min";
    }


    private void SetText(ref string field, string value, [CallerMemberName] string propertyName = "")
    {
        field = value;
        OnPropertyChanged(propertyName);
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "") =>

        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}