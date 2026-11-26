namespace MetroBalance.Core.Contracts;

/// <summary>Standard envelope returned by every MetroBalance API endpoint.</summary>
public class ApiResponse<T>
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }

    public string? ErrorCode { get; init; }

    public DateTime TimestampUtc { get; init; } = DateTime.UtcNow;

    public static ApiResponse<T> Ok(T data, string message = "Success") =>
        new() { Success = true, Data = data, Message = message };

    public static ApiResponse<T> Fail(string errorCode, string message) =>
        new() { Success = false, ErrorCode = errorCode, Message = message };
}

/// <summary>Balance snapshot for a single card.</summary>
public sealed class CardBalanceResult
{
    public required string CardNumber { get; init; }

    public required string CardHolderName { get; init; }

    public required string CardType { get; init; }

    public required string Status { get; init; }

    /// <summary>Balance in Indian Rupees.</summary>
    public required decimal Balance { get; init; }

    /// <summary>Balance formatted as Indian currency, e.g. "₹ 425.00".</summary>
    public required string BalanceFormatted { get; init; }

    public DateTime? LastUpdatedOn { get; init; }

    public string? LastUpdatedOnFormatted { get; init; }

    public DateTime IssuedOn { get; init; }

    public required string CurrencyCode { get; init; }

    /// <summary>True when the numbers above are simulated and not from a live DMRC source.</summary>
    public required bool IsDemoData { get; init; }

    public required string DataSource { get; init; }
}

/// <summary>A single recharge (top-up) entry.</summary>
public sealed class RechargeEntry
{
    public required string TransactionId { get; init; }

    public DateTime Timestamp { get; init; }

    public required string TimestampFormatted { get; init; }

    public required decimal Amount { get; init; }

    public required string AmountFormatted { get; init; }

    public required decimal BalanceAfter { get; init; }

    public required string BalanceAfterFormatted { get; init; }

    public required string Mode { get; init; }

    public required string Channel { get; init; }

    public string? Remarks { get; init; }
}

/// <summary>Recharge history for a card.</summary>
public sealed class RechargeHistoryResult
{
    public required string CardNumber { get; init; }

    public required IReadOnlyList<RechargeEntry> Recharges { get; init; }

    public required decimal TotalRecharged { get; init; }

    public required string TotalRechargedFormatted { get; init; }

    public required int Count { get; init; }

    public required bool IsDemoData { get; init; }
}

/// <summary>A single journey (fare debit) entry.</summary>
public sealed class TransactionEntry
{
    public required string TransactionId { get; init; }

    public DateTime Timestamp { get; init; }

    public required string TimestampFormatted { get; init; }

    public required string FromStation { get; init; }

    public required string ToStation { get; init; }

    public required string Line { get; init; }

    public required decimal Amount { get; init; }

    public required string AmountFormatted { get; init; }

    public required decimal BalanceAfter { get; init; }

    public required string BalanceAfterFormatted { get; init; }

    public required int FareSlab { get; init; }

    public required string EntryGate { get; init; }

    public required string ExitGate { get; init; }
}

/// <summary>Journey history for a card.</summary>
public sealed class TransactionHistoryResult
{
    public required string CardNumber { get; init; }

    public required IReadOnlyList<TransactionEntry> Transactions { get; init; }

    public required decimal TotalSpent { get; init; }

    public required string TotalSpentFormatted { get; init; }

    public required int Count { get; init; }

    public required bool IsDemoData { get; init; }
}

/// <summary>Everything the UI shows on the card detail screen.</summary>
public sealed class CardStatementResult
{
    public required CardBalanceResult Balance { get; init; }

    public required IReadOnlyList<RechargeEntry> Recharges { get; init; }

    public required IReadOnlyList<TransactionEntry> Transactions { get; init; }
}
