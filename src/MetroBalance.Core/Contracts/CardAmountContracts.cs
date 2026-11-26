namespace MetroBalance.Core.Contracts;

/// <summary>
/// Minimal, balance-only result. It carries just the rupee amount plus the few
/// facts needed to explain why no amount is available for a real card.
/// </summary>
public sealed class CardAmountResult
{
    /// <summary>The normalised card number that was looked up.</summary>
    public required string CardNumber { get; init; }

    /// <summary>
    /// True when this response carries a balance value. It is false for real cards:
    /// DMRC publishes no online balance service, so no site can show one.
    /// </summary>
    public required bool BalanceAvailableOnline { get; init; }

    /// <summary>Balance in Indian Rupees, or null when it cannot be read online.</summary>
    public decimal? Amount { get; init; }

    /// <summary>Balance formatted as Indian currency, e.g. "₹ 425.00".</summary>
    public string? AmountFormatted { get; init; }

    /// <summary>Currency of <see cref="Amount"/> - always INR.</summary>
    public string? CurrencyCode { get; init; }

    /// <summary>When the balance was last updated, e.g. "26 Nov 2026, 08:12 AM".</summary>
    public string? LastUpdatedOnFormatted { get; init; }

    /// <summary>True when the amount above is simulated and not from a live DMRC source.</summary>
    public required bool IsDemoData { get; init; }

    /// <summary>Human readable description of where the data came from.</summary>
    public required string DataSource { get; init; }

    /// <summary>The headline message shown to the user.</summary>
    public required string Message { get; init; }

    /// <summary>
    /// Explains why no amount is available. Present only when
    /// <see cref="BalanceAvailableOnline"/> is false.
    /// </summary>
    public string? PolicyNote { get; init; }
}
