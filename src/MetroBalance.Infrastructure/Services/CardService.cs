using MetroBalance.Core.Contracts;
using MetroBalance.Core.Exceptions;
using MetroBalance.Core.Interfaces;
using MetroBalance.Core.Models;

namespace MetroBalance.Infrastructure.Services;

/// <summary>
/// Implements the card enquiry use cases on top of an <see cref="IMetroCardProvider"/>.
/// </summary>
public sealed class CardService : ICardService
{
    private readonly IMetroCardProvider _provider;
    private readonly ICardValidator _validator;

    public CardService(IMetroCardProvider provider, ICardValidator validator)
    {
        _provider = provider;
        _validator = validator;
    }

    public async Task<CardBalanceResult> GetBalanceAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        var card = await GetValidatedCardAsync(cardNumber, cancellationToken).ConfigureAwait(false);
        return BuildBalance(card);
    }

    private CardBalanceResult BuildBalance(SmartCard card) => new()
    {
        CardNumber = card.CardNumber,
        CardHolderName = card.CardHolderName,
        CardType = card.CardType.ToString(),
        Status = card.Status.ToString(),
        Balance = card.Balance,
        BalanceFormatted = MoneyFormatter.Format(card.Balance),
        LastUpdatedOn = card.LastUpdatedOn,
        LastUpdatedOnFormatted = card.LastUpdatedOn is null ? null : MoneyFormatter.FormatTimestamp(card.LastUpdatedOn.Value),
        IssuedOn = card.IssuedOn,
        CurrencyCode = MoneyFormatter.CurrencyCode,
        IsDemoData = _provider.IsSimulated,
        DataSource = _provider.ProviderName
    };

    public async Task<RechargeHistoryResult> GetRechargeHistoryAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        var card = await GetValidatedCardAsync(cardNumber, cancellationToken).ConfigureAwait(false);
        return BuildRecharges(card);
    }

    private RechargeHistoryResult BuildRecharges(SmartCard card)
    {
        var entries = card.Recharges
            .OrderByDescending(r => r.Timestamp)
            .Select(r => new RechargeEntry
            {
                TransactionId = r.TransactionId,
                Timestamp = r.Timestamp,
                TimestampFormatted = MoneyFormatter.FormatTimestamp(r.Timestamp),
                Amount = r.Amount,
                AmountFormatted = MoneyFormatter.Format(r.Amount),
                BalanceAfter = r.BalanceAfter,
                BalanceAfterFormatted = MoneyFormatter.Format(r.BalanceAfter),
                Mode = r.Mode,
                Channel = r.Channel,
                Remarks = r.Remarks
            })
            .ToList();

        var total = entries.Sum(e => e.Amount);

        return new RechargeHistoryResult
        {
            CardNumber = card.CardNumber,
            Recharges = entries,
            TotalRecharged = total,
            TotalRechargedFormatted = MoneyFormatter.Format(total),
            Count = entries.Count,
            IsDemoData = _provider.IsSimulated
        };
    }

    public async Task<TransactionHistoryResult> GetTransactionHistoryAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        var card = await GetValidatedCardAsync(cardNumber, cancellationToken).ConfigureAwait(false);
        return BuildTransactions(card);
    }

    private TransactionHistoryResult BuildTransactions(SmartCard card)
    {
        var entries = card.Transactions
            .OrderByDescending(t => t.Timestamp)
            .Select(t => new TransactionEntry
            {
                TransactionId = t.TransactionId,
                Timestamp = t.Timestamp,
                TimestampFormatted = MoneyFormatter.FormatTimestamp(t.Timestamp),
                FromStation = t.FromStation,
                ToStation = t.ToStation,
                Line = t.Line,
                Amount = t.Amount,
                AmountFormatted = MoneyFormatter.Format(t.Amount),
                BalanceAfter = t.BalanceAfter,
                BalanceAfterFormatted = MoneyFormatter.Format(t.BalanceAfter),
                FareSlab = t.FareSlab,
                EntryGate = t.EntryGate,
                ExitGate = t.ExitGate
            })
            .ToList();

        var total = entries.Sum(e => e.Amount);

        return new TransactionHistoryResult
        {
            CardNumber = card.CardNumber,
            Transactions = entries,
            TotalSpent = total,
            TotalSpentFormatted = MoneyFormatter.Format(total),
            Count = entries.Count,
            IsDemoData = _provider.IsSimulated
        };
    }

    public async Task<CardStatementResult> GetStatementAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        var balance = await GetBalanceAsync(cardNumber, cancellationToken).ConfigureAwait(false);
        var recharges = await GetRechargeHistoryAsync(cardNumber, cancellationToken).ConfigureAwait(false);
        var transactions = await GetTransactionHistoryAsync(cardNumber, cancellationToken).ConfigureAwait(false);

        return new CardStatementResult
        {
            Balance = balance,
            Recharges = recharges.Recharges,
            Transactions = transactions.Transactions
        };
    }

    public Task<IReadOnlyList<string>> GetDemoCardNumbersAsync(CancellationToken cancellationToken = default) =>
        _provider.GetDemoCardNumbersAsync(cancellationToken);

    public async Task<CardEnquiryResult> EnquireAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        var validationMessage = _validator.GetValidationMessage(cardNumber);
        if (validationMessage is not null)
        {
            throw new InvalidCardNumberException(validationMessage);
        }

        var normalized = _validator.Normalize(cardNumber);
        var kind = CardNumberClassifier.Classify(normalized);

        SmartCard? card = null;
        try
        {
            card = await _provider.GetCardAsync(normalized, cancellationToken).ConfigureAwait(false);
        }
        catch (CardNotFoundException)
        {
            card = null;
        }
        catch (CardNotActiveException)
        {
            // The provider knows the card but it is blocked or expired - handled below.
        }

        if (card is not null)
        {
            var balance = BuildBalance(card);
            var recharges = BuildRecharges(card);
            var transactions = BuildTransactions(card);

            return new CardEnquiryResult
            {
                CardNumber = card.CardNumber,
                Found = true,
                IsDemoData = _provider.IsSimulated,
                DataSource = _provider.ProviderName,
                CardKind = kind == CardNumberKind.Unknown ? CardNumberKind.DemoCard : kind,
                CardKindDescription = kind == CardNumberKind.Unknown
                    ? CardNumberClassifier.Describe(CardNumberKind.DemoCard)
                    : CardNumberClassifier.Describe(kind),
                Message = card.Status is CardStatus.Active
                    ? "Statement loaded from the MetroBalance demo dataset (DEMO DATA)."
                    : $"This card is {card.Status}. The statement below is demo data; a real card balance cannot be read online.",
                CardStatus = card.Status.ToString(),
                Statement = new CardStatementResult
                {
                    Balance = balance,
                    Recharges = recharges.Recharges,
                    Transactions = transactions.Transactions
                },
                BalanceAvailableOnline = true,
                Steps = [],
                OfficialLinks = []
            };
        }

        return new CardEnquiryResult
        {
            CardNumber = normalized,
            Found = false,
            IsDemoData = false,
            DataSource = RealCardGuidanceFactory.DataSource,
            CardKind = kind,
            CardKindDescription = CardNumberClassifier.Describe(kind),
            Message = "DMRC does not offer an online card balance service, so this balance cannot be read over the internet. " +
                      "The steps below show how to read it at a station.",
            BalanceAvailableOnline = false,
            PolicyNote = RealCardGuidanceFactory.PolicyNoteFor(kind),
            Steps = RealCardGuidanceFactory.BuildSteps(kind),
            OfficialLinks = RealCardGuidanceFactory.BuildLinks()
        };
    }

    public async Task<CardAmountResult> GetAmountAsync(string cardNumber, CancellationToken cancellationToken = default)
    {
        var enquiry = await EnquireAsync(cardNumber, cancellationToken).ConfigureAwait(false);
        var balance = enquiry.Statement?.Balance;

        return new CardAmountResult
        {
            CardNumber = enquiry.CardNumber,
            BalanceAvailableOnline = enquiry.BalanceAvailableOnline,
            Amount = balance?.Balance,
            AmountFormatted = balance?.BalanceFormatted,
            CurrencyCode = balance?.CurrencyCode ?? MoneyFormatter.CurrencyCode,
            LastUpdatedOnFormatted = balance?.LastUpdatedOnFormatted,
            IsDemoData = enquiry.IsDemoData,
            DataSource = enquiry.DataSource,
            Message = enquiry.BalanceAvailableOnline
                ? "Available balance retrieved from the MetroBalance demo dataset (DEMO DATA)."
                : "DMRC does not offer an online card balance service, so this balance cannot be read over the internet. " +
                  "Read it at an Add Value Machine or the Customer Care counter at any station.",
            PolicyNote = enquiry.PolicyNote
        };
    }

    private async Task<SmartCard> GetValidatedCardAsync(string cardNumber, CancellationToken cancellationToken)
    {
        var message = _validator.GetValidationMessage(cardNumber);
        if (message is not null)
        {
            throw new InvalidCardNumberException(message);
        }

        var normalized = _validator.Normalize(cardNumber);
        var card = await _provider.GetCardAsync(normalized, cancellationToken).ConfigureAwait(false);

        if (card is null)
        {
            throw new CardNotFoundException(normalized);
        }

        if (card.Status is not CardStatus.Active)
        {
            throw new CardNotActiveException(normalized, card.Status.ToString());
        }

        return card;
    }
}
