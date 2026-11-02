namespace Elwise.App.ViewModels;

/// <summary>
/// The prices for one day in the price list, with a title such as "I dag".
/// </summary>
public sealed class PriceGroup : List<PriceRow>
{
    public PriceGroup(string title, IEnumerable<PriceRow> rows)
        : base(rows)
    {
        Title = title;
    }

    public string Title { get; }
}