using Elwise.Core.Prices;
using Elwise.Core.Scheduling;

namespace Elwise.App.Settings;

/// <summary>
/// The user's settings, saved on the phone.
/// </summary>
public sealed class AppSettings
{
    private readonly IPreferences _preferences;

    public AppSettings(IPreferences preferences)
    {
        _preferences = preferences;
    }

    public PriceArea Area
    {
        get => (PriceArea)_preferences.Get(nameof(Area), (int)PriceArea.DK1);
        set => _preferences.Set(nameof(Area), (int)value);
    }

    public int FromHour
    {
        get => _preferences.Get(nameof(FromHour), 0);
        set => _preferences.Set(nameof(FromHour), value);
    }

    public int ToHour
    {
        get => _preferences.Get(nameof(ToHour), 24);
        set => _preferences.Set(nameof(ToHour), value);
    }

    public int DurationMinutes
    {
        get => _preferences.Get(nameof(DurationMinutes), 120);
        set => _preferences.Set(nameof(DurationMinutes), value);
    }

    public AllowedHours AllowedHours => new(FromHour, ToHour);

    public TimeSpan Duration => TimeSpan.FromMinutes(DurationMinutes);
}