using System.Globalization;

namespace MetroBalance.Core.Contracts;

/// <summary>Helpers for formatting Indian Rupee amounts and timestamps.</summary>
public static class MoneyFormatter
{
    public const string CurrencyCode = "INR";

    /// <summary>Formats an amount using the Indian numbering system, e.g. "₹ 1,23,456.00".</summary>
    public static string Format(decimal amount)
    {
        var culture = (CultureInfo)CultureInfo.GetCultureInfo("en-IN");
        return "₹ " + amount.ToString("N2", culture);
    }

    /// <summary>Formats a timestamp for display, e.g. "26 Nov 2026, 10:45 AM".</summary>
    public static string FormatTimestamp(DateTime value) =>
        value.ToString("dd MMM yyyy, hh:mm tt", CultureInfo.InvariantCulture);
}
