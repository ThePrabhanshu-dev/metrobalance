using MetroBalance.Core.Contracts;

namespace MetroBalance.Web.Models;

/// <summary>View model for the card enquiry page.</summary>
public sealed class CardLookupViewModel
{
    public string CardNumber { get; set; } = string.Empty;

    public IReadOnlyList<string> DemoCardNumbers { get; set; } = [];

    public CardStatementResult? Statement { get; set; }

    public bool HasResults => Statement is not null;

    /// <summary>The balance-only answer, used when the page is opened in amount mode.</summary>
    public CardAmountResult? Amount { get; set; }

    /// <summary>True when the page should render only the balance amount.</summary>
    public bool AmountOnly { get; set; }

    /// <summary>The outcome of the enquiry - statement for known cards, guidance for real cards.</summary>
    public CardEnquiryResult? Enquiry { get; set; }

    /// <summary>True when the card is real and MetroBalance must point the user at official DMRC channels.</summary>
    public bool HasGuidance => Enquiry is not null && !Enquiry.Found;

    public string? ErrorMessage { get; set; }
}
