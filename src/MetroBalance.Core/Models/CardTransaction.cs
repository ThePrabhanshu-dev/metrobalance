namespace MetroBalance.Core.Models;

/// <summary>
/// A fare debit applied to the card when a metro journey is completed.
/// </summary>
public sealed class CardTransaction
{
    public required string TransactionId { get; init; }

    public required DateTime Timestamp { get; init; }

    public required string FromStation { get; init; }

    public required string ToStation { get; init; }

    public required string Line { get; init; }

    /// <summary>Fare deducted, in Indian Rupees.</summary>
    public decimal Amount { get; init; }

    /// <summary>Balance remaining after this journey, in Indian Rupees.</summary>
    public decimal BalanceAfter { get; init; }

    public int FareSlab { get; init; }

    public required string EntryGate { get; init; }

    public required string ExitGate { get; init; }
}

/// <summary>
/// A top-up (credit) transaction applied to the card.
/// </summary>
public sealed class RechargeTransaction
{
    public required string TransactionId { get; init; }

    public required DateTime Timestamp { get; init; }

    /// <summary>Amount added, in Indian Rupees.</summary>
    public decimal Amount { get; init; }

    /// <summary>Balance after applying this recharge, in Indian Rupees.</summary>
    public decimal BalanceAfter { get; init; }

    public required string Mode { get; init; }

    public required string Channel { get; init; }

    public string? Remarks { get; init; }
}
