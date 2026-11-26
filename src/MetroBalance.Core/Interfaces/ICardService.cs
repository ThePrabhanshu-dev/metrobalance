using MetroBalance.Core.Contracts;

namespace MetroBalance.Core.Interfaces;

/// <summary>
/// Application level operations for checking a card balance and reading its history.
/// </summary>
public interface ICardService
{
    Task<CardBalanceResult> GetBalanceAsync(string cardNumber, CancellationToken cancellationToken = default);

    Task<RechargeHistoryResult> GetRechargeHistoryAsync(string cardNumber, CancellationToken cancellationToken = default);

    Task<TransactionHistoryResult> GetTransactionHistoryAsync(string cardNumber, CancellationToken cancellationToken = default);

    Task<CardStatementResult> GetStatementAsync(string cardNumber, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetDemoCardNumbersAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Works with any card number. Cards that MetroBalance knows return the full
    /// statement; every other card returns the detected card kind together with the
    /// official DMRC channels for reading a real balance. Never fabricates a balance.
    /// </summary>
    Task<CardEnquiryResult> EnquireAsync(string cardNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns only the balance amount for a card. Cards MetroBalance knows return the
    /// simulated amount; every other card returns "no balance available online" together
    /// with the DMRC policy note - never an invented amount.
    /// </summary>
    Task<CardAmountResult> GetAmountAsync(string cardNumber, CancellationToken cancellationToken = default);
}
