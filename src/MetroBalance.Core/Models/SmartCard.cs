namespace MetroBalance.Core.Models;

/// <summary>
/// Represents a Delhi Metro Smart Card as tracked by the application.
/// </summary>
public sealed class SmartCard
{
    public required string CardNumber { get; init; }

    public required string CardHolderName { get; init; }

    public required CardType CardType { get; init; }

    public required CardStatus Status { get; init; }

    /// <summary>Current account balance expressed in Indian Rupees.</summary>
    public decimal Balance { get; set; }

    public DateTime IssuedOn { get; init; }

    public DateTime? LastUpdatedOn { get; set; }

    /// <summary>Journey (fare debit) transactions, newest first.</summary>
    public List<CardTransaction> Transactions { get; init; } = [];

    /// <summary>Recharge (credit) transactions, newest first.</summary>
    public List<RechargeTransaction> Recharges { get; init; } = [];
}

public enum CardType
{
    Silver,
    Gold,
    Tourist,
    Staff
}

public enum CardStatus
{
    Active,
    Inactive,
    Blocked,
    Expired
}
