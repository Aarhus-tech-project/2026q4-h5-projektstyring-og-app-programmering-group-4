using Elwise.App.Settings;
using Elwise.Core.Prices;

namespace Elwise.App.ViewModels;

/// <summary>
/// Shows the settings and saves every change right away.
/// </summary>
public sealed class SettingsViewModel
{
    private readonly AppSettings _settings;

    public SettingsViewModel(AppSettings settings)
    {
        _settings = settings;
    }

    public List<string> Areas { get; } = ["DK1 – Vestdanmark", "DK2 – Østdanmark"];

    public List<string> FromHours { get; } = Enumerable.Range(0, 24).Select(hour => $"{hour:00}.00").ToList();

    public List<string> ToHours { get; } = Enumerable.Range(1, 24).Select(hour => $"{hour:00}.00").ToList();

    public List<string> Durations { get; } =
        ["1 time", "1 time 30 min", "2 timer", "2 timer 30 min", "3 timer", "3 timer 30 min", "4 timer"];

    public int AreaIndex
    {
        get => (int)_settings.Area;
        set
        {
            if (value >= 0)
            {
                _settings.Area = (PriceArea)value;
            }
        }
    }

    public int FromHourIndex
    {
        get => _settings.FromHour;
        set
        {
            if (value >= 0)
            {
                _settings.FromHour = value;
            }
        }
    }

    public int ToHourIndex
    {
        get => _settings.ToHour - 1;
        set
        {
            if (value >= 0)
            {
                _settings.ToHour = value + 1;
            }
        }
    }

    public int DurationIndex
    {
        get => _settings.DurationMinutes / 30 - 2;
        set
        {
            if (value >= 0)
            {
                _settings.DurationMinutes = (value + 2) * 30;
            }
        }
    }
}