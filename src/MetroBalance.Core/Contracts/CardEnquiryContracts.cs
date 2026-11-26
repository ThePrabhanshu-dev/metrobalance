namespace MetroBalance.Core.Contracts;

/// <summary>
/// How a card number is shaped. DMRC publishes different identifiers for different
/// products, so the shape helps us explain to the user exactly what they entered.
/// </summary>
public enum CardNumberKind
{
    /// <summary>The number could not be classified.</summary>
    Unknown = 0,

    /// <summary>A MetroBalance demo card such as DMRC-DEMO-1001.</summary>
    DemoCard = 1,

    /// <summary>A DMRC "Engraved ID" - 10 or 11 digits printed on the back of the card.</summary>
    EngravedId = 2,

    /// <summary>A 12 digit DMRC Smart Card number.</summary>
    SmartCardNumber = 3,

    /// <summary>A 13-19 digit number, which is the shape of a bank card rather than a DMRC card.</summary>
    BankStyleNumber = 4
}

/// <summary>An official DMRC resource the user can act on.</summary>
public sealed class OfficialLink
{
    public required string Title { get; init; }

    public required string Description { get; init; }

    public required string Url { get; init; }

    /// <summary>True when the link opens the official DMRC top-up portal.</summary>
    public bool IsPrimary { get; init; }
}

/// <summary>
/// The outcome of a card enquiry. When the card is known to MetroBalance the full
/// statement is returned; for a real card the balance cannot be read online, so the
/// result explains how and where to read it.
/// </summary>
public sealed class CardEnquiryResult
{
    /// <summary>The normalised card number that was looked up.</summary>
    public required string CardNumber { get; init; }

    /// <summary>True when a statement is available for this card.</summary>
    public required bool Found { get; init; }

    /// <summary>True when the statement (if any) is simulated demo data.</summary>
    public required bool IsDemoData { get; init; }

    /// <summary>Human readable description of where the data came from.</summary>
    public required string DataSource { get; init; }

    /// <summary>The detected shape of the card number.</summary>
    public required CardNumberKind CardKind { get; init; }

    /// <summary>Plain English description of the detected card number shape.</summary>
    public required string CardKindDescription { get; init; }

    /// <summary>The headline message shown to the user.</summary>
    public required string Message { get; init; }

    /// <summary>True when the card status was found but the card cannot be used.</summary>
    public string? CardStatus { get; init; }

    /// <summary>Balance, history and journeys - present only for cards MetroBalance knows.</summary>
    public CardStatementResult? Statement { get; init; }

    /// <summary>
    /// True when this response carries a balance value. It is false for real cards:
    /// DMRC publishes no online balance service, so no site can show one.
    /// </summary>
    public required bool BalanceAvailableOnline { get; init; }

    /// <summary>Explains the DMRC policy on online balance checks.</summary>
    public string? PolicyNote { get; init; }

    /// <summary>Step by step instructions for reading the balance of a real card.</summary>
    public IReadOnlyList<string> Steps { get; init; } = [];

    /// <summary>Official DMRC resources for this card.</summary>
    public IReadOnlyList<OfficialLink> OfficialLinks { get; init; } = [];
}
