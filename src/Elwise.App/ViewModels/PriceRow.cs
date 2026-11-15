namespace Elwise.App.ViewModels;

/// <summary>
/// One row in the price list.
/// </summary>
/// <param name="Time">The Danish time, for example "02.15".</param>
/// <param name="Price">The price in kroner, for example "0,85 kr.".</param>
/// <param name="IsInWindow">True if the quarter is part of the cheapest window.</param>
public sealed record PriceRow(string Time, string Price, bool IsInWindow);