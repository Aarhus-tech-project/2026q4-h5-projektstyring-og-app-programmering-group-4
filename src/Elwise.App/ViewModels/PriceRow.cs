namespace Elwise.App.ViewModels;

/// <summary>
/// One row in the price list: the Danish time and the price in kroner.
/// </summary>
public sealed record PriceRow(string Time, string Price);